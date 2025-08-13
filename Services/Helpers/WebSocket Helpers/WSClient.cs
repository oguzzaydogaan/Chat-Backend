using Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Services.DTOs;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Services
{
    public class WSClient
    {
        public WebSocket _client;
        private readonly IServiceScopeFactory _scopeFactory;

        public WSClient(WebSocket client, IServiceScopeFactory scopeFactory)
        {
            _client = client;
            _scopeFactory = scopeFactory;
        }
        public async Task ListenClient(DateTime validTo)
        {
            var buffer = new byte[1024 * 256];
            WebSocketReceiveResult receiveResult;
            ConnectionFactory factory;

            using (var scope = _scopeFactory.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                factory = new ConnectionFactory
                {
                    HostName = configuration["RabbitMQ:HostName"] ?? throw new ConfigurationException("MQ hostname is null."),
                    Port = int.Parse(configuration["RabbitMQ:Port"] ?? throw new ConfigurationException("MQ port is null.")),
                    UserName = configuration["RabbitMQ:UserName"] ?? throw new ConfigurationException("MQ username is null."),
                    Password = configuration["RabbitMQ:Password"] ?? throw new ConfigurationException("MQ password is null.")
                };
            }
            
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "raw_messages", durable: true, exclusive: false,
                autoDelete: false, arguments: null);

            while (true)
            {
                using var ms = new MemoryStream();
                do
                {
                    receiveResult = await _client.ReceiveAsync(
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
                catch(JsonException ex)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<WSClient>>();
                    logger.LogError($"Websocket json error: {ex.Message}");
                    await SendErrorAsync("Bad message format.");
                }
                catch (Exception ex)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<WSClient>>();
                    logger.LogError($"Websocket mesajı kuyruğa yazılırken hata: {ex.Message}");
                    await SendErrorAsync("Something went wrong.");
                }
            }
        }

        public async Task SendErrorAsync(string ex, int id = -1)
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

            await _client.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
        }
        public async Task CloseAsync(string reason)
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