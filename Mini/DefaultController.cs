using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/")]
public class DefaultController(DiscordSocketClient client, ILogger<WebhookController> logger) : ControllerBase
{
    private readonly DiscordSocketClient _client = client;
    private readonly ILogger<WebhookController> _logger = logger;

    [HttpGet("/Health")]
    public async Task<IActionResult> Health()
    {
        return Ok();
    }
}