using App.Services.Queues.Messages;
using App.Services.Queues.Publishers;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;

namespace App.API.Controllers
{
    public class TestController(IRabbitMQPublisher rabbitMqPublisher) : CustomBaseController
    {
        [HttpGet("send")]
        public async Task<IActionResult> SendTestMessage()
        {
            var message = new ExcelExportMessage
            {
                ExportType = "test",
                //RequestedBy = "admin",
                RequestedAt = DateTime.Now
            };
            await rabbitMqPublisher.SendMessageAsync(message);
            return Ok("Mesaj gönderildi!");
        }

        [HttpPost("category")]
        public async Task<IActionResult> ExportCategoryToExcel()
        {
            var message = new ExcelExportMessage
            {
                ExportType = "category",
                RequestedBy = "admin",
                RequestedAt = DateTime.Now
            };

            await rabbitMqPublisher.SendMessageAsync(message);

            return Ok(new { message = "Export talebi kuyruğa gönderildi." });
        }
    }
}
