using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RawMessageWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        private async Task InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            string mainQueueName = "raw_messages";
            await _channel.QueueDeclareAsync(queue: mainQueueName, durable: true, exclusive: false,
                autoDelete: false, arguments: null);

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("RabbitMQ bağlantısı kuruldu ve 'raw_messages' kuyruğu oluşturuldu.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeRabbitMQ();
            var consumer = new AsyncEventingBasicConsumer(_channel!);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);
                using (var scope = _scopeFactory.CreateScope())
                {
                    var processMessageService = scope.ServiceProvider.GetRequiredService<ProcessMessage>();
                    try
                    {
                        await processMessageService.ProcessMessageAsync(message);
                        await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Websocket mesajı işlenirken hata: " + ex.Message);
                        await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
            };

            await _channel!.BasicConsumeAsync("raw_messages", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
