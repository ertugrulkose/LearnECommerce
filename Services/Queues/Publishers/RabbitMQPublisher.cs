using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using App.Services.Queues.Constants;
using App.Services.Queues.Messages;
using System.Text.Json;

namespace App.Services.Queues.Publishers
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqSettings _settings;

        public RabbitMQPublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;

            _factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };
        }

        public async Task SendMessageAsync(ExcelExportMessage message)
        {
            var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueNames.ExportCategoryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: QueueNames.ExportCategoryQueue,
                body: body
            );

            await channel.CloseAsync();
            await connection.CloseAsync();
        }

    }
}