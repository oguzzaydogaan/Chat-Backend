using Microsoft.Extensions.DependencyInjection;
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
        private WSClientListManager _wsClientListManager;
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
                    await _wsClientListManager.RemoveClient(id);
                }
                var info = await ConvertRequest(buffer, receiveResult)
                ;
                await SendMessageToClients(info.Bytes, info.Users);
            }
            while (!receiveResult.CloseStatus.HasValue);

            await _wsClientListManager.RemoveClient(id);

        }

        public async Task<BytesWithUsersDTO> ConvertRequest(byte[] buffer, WebSocketReceiveResult receiveResult)
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var messageService = serviceScope.ServiceProvider.GetService<MessageService>();
                var messageString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                SocketMessageDTO messageJson = JsonSerializer.Deserialize<SocketMessageDTO>(messageString);
                ResponseSocketMessageDTO socketMessage = new ResponseSocketMessageDTO();
                MessageWithUsersDTO mWithUsers = new MessageWithUsersDTO();

                if (messageJson?.Type == "Send-Message")
                {
                    socketMessage.Type = "Send-Message";
                    mWithUsers = await messageService.AddMessageAsync(messageJson.Payload!.ToMessage()) ?? throw new Exception();
                    socketMessage.Payload = mWithUsers.Message.ToMessageForChatDTO();
                }
                else
                {
                    if (messageJson != null)
                    {
                        int mid = (int)messageJson!.Payload!.MessageID!;
                        socketMessage.Type = "Delete-Message";
                        mWithUsers = await messageService.DeleteMessageAsync(mid);
                        socketMessage.Payload = mWithUsers.Message.ToMessageForChatDTO();
                    }
                }

                var json = JsonSerializer.Serialize(socketMessage);
                var bytes = Encoding.UTF8.GetBytes(json);
                return new BytesWithUsersDTO { Bytes = bytes, Users = mWithUsers.Users };
            }
        }

        public async Task SendMessageToClients(byte[] bytes, ICollection<User> users)
        {
            foreach (var user in users)//TODO add filter for message owners
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

        public async Task Close()
        {
            if (_client.State == WebSocketState.Open)
            {
                await _client.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Kapatıldı.",
                    CancellationToken.None);
            }
        }
    }
}