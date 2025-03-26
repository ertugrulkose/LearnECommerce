using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace App.Services.Queues.Consumers;

public class RabbitMqConsumer : BackgroundService
{
    private readonly ConnectionFactory _factory;
    private readonly RabbitMqSettings _settings;

    public RabbitMqConsumer(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
            Port = _settings.Port
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // Kuyruk tanımı (gelen mesajlar için)
        await channel.QueueDeclareAsync(
            queue: "example-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"📩 Mesaj alındı: {message}");

                // Örnek deserialize işlemi (gerekirse)
                // var data = JsonSerializer.Deserialize<YourDtoType>(message);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hata: {ex.Message}");
            }
        };

        // Consumer başlatılıyor
        await channel.BasicConsumeAsync(
            queue: "example-queue",
            autoAck: true,
            consumer: consumer
        );

        // Servis çalıştığı sürece burada takılsın
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        // Temizlik
        await channel.CloseAsync();
        await connection.CloseAsync();
    }
}
