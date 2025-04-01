using App.Services.Categories;
using App.Services.Queues.Constants;
using App.Services.Queues.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace App.Services.Queues.Consumers
{
    public class ExcelExportConsumer : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;


        public ExcelExportConsumer(IOptions<RabbitMqSettings> options, IServiceScopeFactory scopeFactory)
        {
            _settings = options.Value;
            _scopeFactory = scopeFactory;

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
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueNames.ExportCategoryQueue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<ExcelExportMessage>(json);

                    Console.WriteLine($"📥 Excel export isteği alındı. Tür: {message?.ExportType}");

                    if (message?.ExportType == "category")
                    {
                        // 🔄 Scoped service oluştur // buraya sonra bakılcak
                        using var scope = _scopeFactory.CreateScope();

                        var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();

                        var excelBytes = await categoryService.ReportCategoriesToExcelAsync(
                            message.Filters,
                            message.Columns,
                            message.Sort
                        );

                        var fileName = $"kategori-raporu-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                        var exportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");

                        if (!Directory.Exists(exportPath))
                            Directory.CreateDirectory(exportPath);

                        var fullPath = Path.Combine(exportPath, fileName);
                        await File.WriteAllBytesAsync(fullPath, excelBytes);

                        Console.WriteLine($"✅ Excel dosyası oluşturuldu: /exports/{fileName}");

                        // TODO: SignalR ile kullanıcıya bildir

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Consumer hata: {ex.Message}");
                }
            };

            await channel.BasicConsumeAsync(
                queue: QueueNames.ExportCategoryQueue,
                autoAck: true,
                consumer: consumer
            );

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
