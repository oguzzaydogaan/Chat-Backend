using Exceptions;
using RabbitMQ.Client;
using System.Net.WebSockets;

namespace Services
{
    public class WSClient
    {
        public WebSocket _client;

        public WSClient(WebSocket client)
        {
            _client = client;
        }
        public async Task ListenClient(DateTime validTo)
        {
            var buffer = new byte[1024 * 256];
            WebSocketReceiveResult receiveResult;

            var factory = new ConnectionFactory { HostName = "localhost" };
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

                ms.Seek(0, SeekOrigin.Begin);
                var body = ms.ToArray();

                var properties = new BasicProperties
                {
                    Persistent = true
                };

                try
                {
                    await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "raw_messages", mandatory: true,
                    basicProperties: properties, body: body);
                }
                catch (Exception ex)
                {

                }
            }
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