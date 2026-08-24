using Discord;
using Discord.Interactions;

public class InquiryModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<InquiryModule> _logger;

    public InquiryModule(ILogger<InquiryModule> logger)
    {
        _logger = logger;
    }

    [SlashCommand("문의", "개발자/관리자에게 익명으로 문의나 건의사항을 남깁니다.")]
    public async Task HandleInquiry([Summary("내용", "전달할 문의 내용을 적어주세요.")] string content)
    {
        // 1. 유저 본인의 화면에만 보일 접수 확인용 임베드 카드
        var userConfirmEmbed = new EmbedBuilder()
            .WithTitle("📩 문의사항이 성공적으로 접수되었습니다")
            .WithDescription("관리자가 확인 후 답변을 남기면 이 채널에 알림이 표시됩니다.")
            .AddField("📝 작성한 문의 내용", content, false)
            .AddField("⏱️ 접수 일시", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .Build();

        await RespondAsync(embed: userConfirmEmbed, ephemeral: true);

        try
        {
            // .env에서 관리자 ID 가져오기
            string adminIdStr = Environment.GetEnvironmentVariable("ADMIN_USER_ID") ?? "0";
            if (!ulong.TryParse(adminIdStr, out ulong adminUserId) || adminUserId == 0)
            {
                _logger.LogError("ADMIN_USER_ID가 .env에 올바르게 설정되지 않았습니다.");
                return;
            }

            var adminUser = await Context.Client.GetUserAsync(adminUserId);
            if (adminUser != null)
            {
                var dmChannel = await adminUser.CreateDMChannelAsync();

                // 관리자 DM으로 알림 카드 발송
                var adminEmbed = new EmbedBuilder()
                    .WithTitle("🚨 새로운 문의가 도착했습니다!")
                    .AddField("보낸 사람", $"{Context.User.Mention} (`{Context.User.Username}`, ID: `{Context.User.Id}`)", false)
                    .AddField("소속 서버/채널", $"{Context.Guild?.Name ?? "DM"} / <#{Context.Channel.Id}>", false)
                    .WithDescription(content)
                    .WithColor(Color.Orange)
                    .WithCurrentTimestamp()
                    .Build();

                await dmChannel.SendMessageAsync(embed: adminEmbed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "문의 사항 전달 중 오류가 발생했습니다.");
        }
    }
}