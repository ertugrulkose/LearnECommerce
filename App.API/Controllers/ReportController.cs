using App.Services.Queues.Messages;
using App.Services.Queues.Publishers;
using Microsoft.AspNetCore.Mvc;

namespace App.API.Controllers
{
    public class ReportController(IRabbitMQPublisher rabbitMqPublisher) : CustomBaseController
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
        public async Task<IActionResult> ExportCategoryToExcel([FromBody] ExcelExportMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.ExportType))
                return BadRequest("Eksik export bilgisi.");

            await rabbitMqPublisher.SendMessageAsync(message);

            return Ok(new { message = "Export talebi kuyruğa gönderildi." });
        }
    }
}
