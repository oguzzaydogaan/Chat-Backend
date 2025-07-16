using Exceptions;
using Microsoft.EntityFrameworkCore;
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
                try
                {
                    var msg = await ConvertRequest(buffer, receiveResult);
                    if (msg.Bytes == null || msg.Users == null)
                    {
                        throw new ArgumentNullException("Msg error.");
                    }
                    await SendMessageToClients(msg.Bytes, msg.Users);
                }
                catch (ArgumentNullException ex)
                {
                    await SendErrorToClient(ex.Message);
                }
                catch (DbUpdateException)
                {
                    await SendErrorToClient("Database error.");
                }
                catch (NotSupportedException)
                {
                    await SendErrorToClient("JSON error.");
                }
                catch (Exception ex)
                {
                    await SendErrorToClient(ex.Message);
                }
            }
            while (!receiveResult.CloseStatus.HasValue);

            await _wsClientListManager.RemoveClient(id, "Websocket status is closed.");

        }

        public async Task<BytesWithUsersDTO> ConvertRequest(byte[] buffer, WebSocketReceiveResult receiveResult)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var messageService = serviceScope.ServiceProvider.GetService<MessageService>();
            var chatService = serviceScope.ServiceProvider.GetService<ChatService>();
            if (messageService == null || chatService == null)
            {
                throw new ArgumentNullException("An error occured");
            }
            var messageString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            RequestSocketMessageDTO? messageJson = JsonSerializer.Deserialize<RequestSocketMessageDTO>(messageString);
            if (string.IsNullOrEmpty(messageJson?.ToString()))
            {
                throw new ArgumentNullException("Message couldn't send");
            }
            ResponseSocketMessageDTO socketMessage = new();
            MessageWithUsersDTO? mWithUsers = new();

            if (messageJson.Type == "Send-Message")
            {
                socketMessage.Type = "Send-Message";
                mWithUsers = await messageService.AddAsync(messageJson.Payload.ToMessage());
                if (mWithUsers == null || mWithUsers.Message == null)
                {
                    throw new ArgumentNullException("Message couldn't send");
                }
                socketMessage.Payload.Message = mWithUsers.Message.ToMessageForChatDTO();
            }
            else if (messageJson.Type == "Delete-Message")
            {
                if (messageJson.Payload.MessageId == null)
                {
                    throw new ArgumentNullException("Message couldn't delete");
                }
                int mid = (int)messageJson.Payload.MessageId;
                socketMessage.Type = "Delete-Message";
                mWithUsers = await messageService.DeleteMessageAsync(mid);
                if (mWithUsers == null || mWithUsers.Message == null)
                {
                    throw new ArgumentNullException("Message couldn't delete");
                }
                socketMessage.Payload.Message = mWithUsers.Message.ToMessageForChatDTO();
            }
            else if (messageJson.Type == "New-Chat")
            {
                socketMessage.Type = "New-Chat";
                if (messageJson.Payload.UserIds == null)
                {
                    throw new ArgumentNullException("Chat couldn't create");
                }
                var chat = await chatService.AddChatAsync(messageJson.Payload.UserIds);
                if (chat == null)
                {
                    throw new ChatNotFoundException();
                }
                mWithUsers.Users = chat.Users;
                socketMessage.Payload.Chat = chat.EntityToChatDTO();
            }
            else if (messageJson.Type == "New-UserToChat")
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


        public async Task SendErrorToClient(string ex)
        {
            ResponseSocketMessageDTO message = new()
            {
                Type = "Error"
            };
            message.Payload.Error = ex;
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _client.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
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