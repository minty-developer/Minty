using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WebhookController(DiscordSocketClient client, ILogger<WebhookController> logger) : ControllerBase
{
    private readonly DiscordSocketClient _client = client;
    private readonly ILogger<WebhookController> _logger = logger;

    // POST /api/webhook/alert
    [HttpPost("alert")]
    public async Task<IActionResult> ReceiveAlert([FromBody] WebhookPayload payload)
    {
        if (payload == null || string.IsNullOrEmpty(payload.Message) || payload.ChannelId == 0)
        {
            return BadRequest("잘못된 데이터 페이로드입니다. Message와 ChannelId를 확인해주세요.");
        }

        _logger.LogInformation($"외부 웹훅 수신됨 (대상 채널: {payload.ChannelId}): {payload.Message}");

        // 클라이언트에서 지정해 준 채널 ID로 봇이 찾아가서 메시지 전송
        if (_client.GetChannel(payload.ChannelId) is not ISocketMessageChannel channel)
        {
            return NotFound($"ID가 {payload.ChannelId}인 채널을 찾을 수 없거나 봇이 접근할 수 없습니다.");
        }

        await channel.SendMessageAsync($"🚨 [외부 알림] {payload.Message}");

        return Ok(new { status = "success", targetChannel = payload.ChannelId, receivedAt = DateTime.Now });
    }
}

// 외부에서 보낼 데이터 구조체 (Body)
public class WebhookPayload
{
    public ulong ChannelId { get; set; }  // 메시지를 쏠 대상 디스코드 채널 ID
    public string? Message { get; set; }   // 보낼 내용
}