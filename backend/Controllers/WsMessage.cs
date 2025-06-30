using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Entities;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("/ws/message")]
    public class WsMessage : ControllerBase
    {
        public WsMessage(List<WebSocket> clients)
        {
            _clients = clients;
        }
        public static List<WebSocket>? _clients;
        public static List<Message> _messages = new List<Message>();
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                if (webSocket.State == WebSocketState.Open)
                {
                    foreach (var message in _messages)
                    {
                        var json = JsonSerializer.Serialize(message);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
                _clients?.Add(webSocket);
                await Echo(webSocket);
            }
            else
            {
                Console.WriteLine("Bu endpoint sadece WebSocket istekleri için kullanılabilir.");
            }
        }
        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                var messageJson = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                if (message != null)
                    _messages.Add(message);
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                foreach (var client in _clients!)
                {
                    await client.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
                }
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
            _clients!.Remove(webSocket);
        }
    }
}
