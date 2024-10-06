using Microsoft.AspNetCore.Mvc;

namespace SharpQueue.Http.Controllers;

[ApiController]
[Route("queue/{queueName}/messages")]
public class QueueController(QueueManager queueManager) : ControllerBase
{
    private readonly QueueManager _queueManager = queueManager;

    [HttpPost]
    public async Task<IActionResult> AddMessage(string queueName, [FromBody] string message)
    {
        await _queueManager.AddMessageAsync(queueName, message);
        return Ok(new { Status = "Message enqueued successfully" });
    }

    [HttpGet("consume")]
    public async Task<IActionResult> ConsumeMessage(string queueName)
    {
        var message = await _queueManager.ConsumeMessageAsync(queueName);
        if (message == null) return NoContent();

        return Ok(new { Message = message.GetBodyAsString() });
    }
}
