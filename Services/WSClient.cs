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
        private MessageService _messageService;
        private WSClientListManager _wsManager;
        private readonly WebSocket _client;

        public WSClient(WSClientListManager wsManager, WebSocket client, MessageService messageService)
        {
            _messageService = messageService;
            _wsManager = wsManager;
            _client = client;
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
                    await _wsManager.RemoveClient(id);
                }
                var bytes = await ConvertRequest(buffer, receiveResult);
                await SendMessageToClients(bytes);
            }
            while (!receiveResult.CloseStatus.HasValue);

            await _wsManager.RemoveClient(id);

        }

        public async Task<byte[]> ConvertRequest(byte[] buffer, WebSocketReceiveResult receiveResult)
        {
            var messageString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            SocketMessageDTO? messageJson = JsonSerializer.Deserialize<SocketMessageDTO>(messageString);
            ResponseSocketMessageDTO? socketMessage = new ResponseSocketMessageDTO();
            Message msg;
            if (messageJson?.Type == "Send-Message")
            {
                socketMessage.Type = "Send-Message";
                msg = await _messageService.AddMessageAsync(messageJson.Payload!.ToMessage()) ?? throw new Exception();
                socketMessage.Payload = msg.ToMessageForChatDTO();
            }
            else
            {
                if (messageJson != null)
                {
                    int mid = (int)messageJson!.Payload!.MessageID!;
                    socketMessage.Type = "Delete-Message";
                    msg = await _messageService.DeleteMessageAsync(mid);
                    socketMessage.Payload = msg.ToMessageForChatDTO();
                }
            }
            var json = JsonSerializer.Serialize(socketMessage);
            var bytes = Encoding.UTF8.GetBytes(json);
            return bytes;
        }

        public async Task SendMessageToClients(byte[] bytes)
        {
            foreach (var ws in _wsManager.Clients.Values)//TODO add filter for message owners
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