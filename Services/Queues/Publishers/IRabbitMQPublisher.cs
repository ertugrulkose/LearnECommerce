using App.Services.Queues.Messages;

namespace App.Services.Queues.Publishers
{
    public interface IRabbitMQPublisher
    {
        Task SendMessageAsync(ExcelExportMessage message);
    }
}
