using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Services
{
    public class WSClient
    {
        private readonly WSClientListManager _wsClientListManager;
        private readonly WebSocket _client;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WSClient(WSClientListManager wsClientListManager, WebSocket client, IServiceScopeFactory serviceScopeFactory)
        {
            _wsClientListManager = wsClientListManager;
            _client = client;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task ListenClient(int id, DateTime validTo)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult receiveResult;

            do
            {
                receiveResult = await _client.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
                if (validTo < DateTime.UtcNow)
                {
                    throw new Exception("Token expired.");
                }
                var info = await ConvertRequest(buffer, receiveResult);
                if (info == null || info.Bytes == null || info.Users == null)
                {
                    throw new Exception("Message or Users can not be null.");
                }

                await SendMessageToClients(info.Bytes, info.Users);
            }
            while (!receiveResult.CloseStatus.HasValue);

            await _wsClientListManager.RemoveClient(id, "Websocket status is closed.");

        }

        public async Task<BytesWithUsersDTO?> ConvertRequest(byte[] buffer, WebSocketReceiveResult receiveResult)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var messageService = serviceScope.ServiceProvider.GetService<MessageService>();
            var chatService = serviceScope.ServiceProvider.GetService<ChatService>();
            if (messageService == null || chatService == null)
            {
                return null;
            }
            var messageString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            RequestSocketMessageDTO? messageJson = JsonSerializer.Deserialize<RequestSocketMessageDTO>(messageString);
            if (string.IsNullOrEmpty(messageJson?.ToString()))
            {
                return null;
            }
            ResponseSocketMessageDTO socketMessage = new();
            MessageWithUsersDTO mWithUsers = new();

            if (messageJson?.Type == "Send-Message")
            {
                socketMessage.Type = "Send-Message";
                mWithUsers = await messageService!.AddMessageAsync(messageJson.Payload!.ToMessage());
                socketMessage.Payload.Message = mWithUsers.Message!.ToMessageForChatDTO();
            }
            else if (messageJson?.Type == "Delete-Message")
            {
                int mid = (int)messageJson!.Payload!.MessageId!;
                socketMessage.Type = "Delete-Message";
                mWithUsers = await messageService!.DeleteMessageAsync(mid);
                socketMessage.Payload.Message = mWithUsers.Message!.ToMessageForChatDTO();
            }
            else if (messageJson?.Type == "New-Chat")
            {
                socketMessage.Type = "New-Chat";
                if (messageJson.Payload.UserIds == null)
                {
                    return null;
                }
                var chat = await chatService.AddChatAsync(messageJson.Payload.UserIds);
                if (chat == null)
                {
                    return null;
                }
                mWithUsers.Users = chat.Users;
                socketMessage.Payload.Chat = chat.EntityToChatDTO();
            }
            else if (messageJson?.Type == "New-UserToChat")
            {
                socketMessage.Type = "New-UserToChat";
                var chat = await chatService.AddUserToChatAsync(messageJson.Payload.ChatId, messageJson.Payload.UserId);
                mWithUsers.Users = chat.Users;
                socketMessage.Payload.Chat = chat.EntityToChatDTO();
            }

            var json = JsonSerializer.Serialize(socketMessage);
            var bytes = Encoding.UTF8.GetBytes(json);
            return new BytesWithUsersDTO { Bytes = bytes, Users = mWithUsers.Users };
        }

        public async Task SendMessageToClients(byte[] bytes, ICollection<User> users)
        {
            foreach (var user in users)
            {
                _wsClientListManager.Clients.TryGetValue(user.Id, out var ws);
                if (ws != null)
                {
                    if (ws._client.State == WebSocketState.Open)
                    {
                        await ws._client.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
            }
        }

        public async Task Close(string reason)
        {
            if (_client.State == WebSocketState.Open)
            {
                await _client.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    reason,
                    CancellationToken.None);
            }
        }
    }
}