using Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Repositories.Entities;
using Services.DTOs;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Services.Helpers.WebSocket_Helpers
{
    public class WSManager
    {
        private readonly IConfiguration _configuration;
        private readonly WSListManager _wsListManager;
        private readonly ILogger<WSManager> _logger;
        public WSManager(IConfiguration configuration, WSListManager wSClientListManager, ILogger<WSManager> logger)
        {
            _configuration = configuration;
            _wsListManager = wSClientListManager;
            _logger = logger;
        }
        public async Task ListenClientAsync(WebSocket webSocket, DateTime validTo)
        {
            var buffer = new byte[1024 * 256];
            WebSocketReceiveResult receiveResult;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? throw new ConfigurationException("MQ hostname is null."),
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? throw new ConfigurationException("MQ port is null.")),
                UserName = _configuration["RabbitMQ:UserName"] ?? throw new ConfigurationException("MQ username is null."),
                Password = _configuration["RabbitMQ:Password"] ?? throw new ConfigurationException("MQ password is null.")
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "raw_messages", durable: true, exclusive: false,
                autoDelete: false, arguments: null);

            while (true)
            {
                using var ms = new MemoryStream();
                do
                {
                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);

                    ms.Write(buffer, 0, receiveResult.Count);

                } while (!receiveResult.EndOfMessage);
                if (receiveResult.CloseStatus.HasValue)
                {
                    break;
                }
                if (validTo < DateTime.UtcNow)
                {
                    throw new TokenExpiredException();
                }

                try
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var body = ms.ToArray();
                    var req = JsonSerializer.Deserialize<RequestSocketDTO>(Encoding.UTF8.GetString(body));

                    var properties = new BasicProperties
                    {
                        Persistent = true
                    };
                    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "raw_messages", mandatory: true,
                    basicProperties: properties, body: body);
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"Websocket json error: {ex.Message}");
                    await SendErrorAsync(webSocket,"Bad message format.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Websocket mesajı kuyruğa yazılırken hata: {ex.Message}");
                    await SendErrorAsync(webSocket, "Something went wrong.");
                }
            }
        }
        public async Task SendMessageToUsersAsync(byte[] bytes, ICollection<User> recievers)
        {
            foreach (var reciever in recievers)
            {
                _wsListManager.Clients.TryGetValue(reciever.Id, out var webSocket);
                if (webSocket != null)
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
            }
        }
        public async Task SendErrorAsync(WebSocket webSocket, string ex, int id = -1)
        {
            ResponseSocketDTO message = new()
            {
                Type = ResponseEventType.Error,
                Payload =
                {
                    Error = ex,
                    Chat = new()
                    {
                        Id=id
                    }
                }
            };

            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await webSocket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
        }
        public async Task CloseAsync(int id, WebSocket webSocket, string reason)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    reason,
                    CancellationToken.None);
            }
        }
    }
}
