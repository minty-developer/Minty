using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Collections.Concurrent;

public partial class BotWorker : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BotWorker> _logger;

    public BotWorker(DiscordSocketClient client, InteractionService commands, IServiceProvider services, IConfiguration configuration, ILogger<BotWorker> logger)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try 
        {
            _client.Log += LogAsync;
            _commands.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += HandleAdminReplyAsync;

            _client.InteractionCreated += async interaction =>
            {
                if (interaction is SocketMessageComponent componentInteraction)
                {
                    await HandleButtonInteractionAsync(componentInteraction);
                }
                else
                {
                    var ctx = new SocketInteractionContext(_client, interaction);
                    await _commands.ExecuteCommandAsync(ctx, _services);
                }
            };

            var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? _configuration["Discord:Token"];
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("디스코드 봇 토큰이 설정되지 않았습니다.");
                return;
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        } 
        catch (Discord.Net.HttpException ex) when ((int)ex.HttpCode == 429)
        {
            _logger.LogError(ex, "디스코드 API Rate Limit에 도달했습니다. 일정 시간 후 재시도해야 합니다.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "봇을 시작하는 중 오류가 발생했습니다.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try 
        {
            await _client.StopAsync();
        } 
        catch (Exception ex)
        {
            _logger.LogError(ex, "봇을 멈추는 중 오류가 발생했습니다.");
        }
    }

    private Task LogAsync(LogMessage message)
    {
        _logger.LogInformation(message.ToString());
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        _logger.LogInformation($"{_client.CurrentUser} 봇이 성공적으로 로그인했습니다!");

        try
        {
            await _commands.RegisterCommandsGloballyAsync();
            _logger.LogInformation("글로벌 슬래시 커맨드가 성공적으로 등록되었습니다.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "슬래시 명령어 등록 중 오류가 발생했습니다.");
        }
    }

    private async Task HandleAdminReplyAsync(SocketMessage rawMessage)
    {
        if (rawMessage.Author.Id == _client.CurrentUser.Id) return;

        string adminIdStr = Environment.GetEnvironmentVariable("ADMIN_USER_ID") ?? _configuration["AdminUserId"] ?? "0";
        if (!ulong.TryParse(adminIdStr, out ulong adminUserId) || adminUserId == 0) return;

        if (rawMessage.Author.Id != adminUserId) return;
        if (rawMessage.Reference == null || !rawMessage.Reference.MessageId.IsSpecified) return;

        try
        {
            var channel = rawMessage.Channel;
            var repliedMessage = await channel.GetMessageAsync(rawMessage.Reference.MessageId.Value);

            if (repliedMessage != null && repliedMessage.Embeds.Count > 0)
            {
                var embed = repliedMessage.Embeds.First();
                
                string userFieldText = embed.Fields
                    .Where(f => f.Name == "보낸 사람")
                    .Select(f => f.Value)
                    .FirstOrDefault() ?? string.Empty;
                string channelFieldText = embed.Fields.FirstOrDefault(f => f.Name == "소속 서버/채널").Value ?? string.Empty;
                
                var userMatch = System.Text.RegularExpressions.Regex.Match(userFieldText, @"ID:\s*`?(\d+)`?");
                var channelMatch = System.Text.RegularExpressions.Regex.Match(channelFieldText, @"<#(\d+)>");

                if (userMatch.Success && ulong.TryParse(userMatch.Groups[1].Value, out ulong targetUserId))
                {
                    IMessageChannel? targetChannel = null;

                    if (channelMatch.Success && ulong.TryParse(channelMatch.Groups[1].Value, out ulong targetChannelId))
                    {
                        targetChannel = await _client.GetChannelAsync(targetChannelId) as IMessageChannel;
                    }

                    if (targetChannel != null)
                    {
                        string originalQuestion = embed.Description ?? "내용 없음";
                        string adminAnswer = rawMessage.Content;

                        var buttonBuilder = new ComponentBuilder()
                            .WithButton("💬 답변 확인하기", $"btn_reply_{targetUserId}", ButtonStyle.Primary);

                        var noticeEmbed = new EmbedBuilder()
                            .WithTitle("📢 문의하신 내용에 대한 답변이 도착했습니다!")
                            .WithDescription($"{MentionUtils.MentionUser(targetUserId)}님, 아래 버튼을 누르면 본인만 답변을 확인할 수 있습니다.")
                            .WithColor(Color.Green)
                            .WithCurrentTimestamp()
                            .Build();

                        var sentMsg = await targetChannel.SendMessageAsync(embed: noticeEmbed, components: buttonBuilder.Build());

                        // 캐시에 안전하게 보관
                        ReplyCache.Store(sentMsg.Id, targetUserId, originalQuestion, adminAnswer);

                        await rawMessage.AddReactionAsync(new Emoji("✅"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "관리자 답변 전달 중 오류가 발생했습니다.");
            await rawMessage.AddReactionAsync(new Emoji("❌"));
        }
    }

    private async Task HandleButtonInteractionAsync(SocketMessageComponent component)
    {
        if (component.Data.CustomId.StartsWith("btn_reply_"))
        {
            ulong msgId = component.Message.Id;

            if (ReplyCache.TryGet(msgId, out var cacheData))
            {
                // 본인이 맞는지 검증
                if (component.User.Id != cacheData.TargetUserId)
                {
                    await component.RespondAsync("❌ 본인이 신청한 문의만 확인할 수 있습니다.", ephemeral: true);
                    return;
                }

                // 본인이 맞다면 에페머럴 답변 출력
                var replyEmbed = new EmbedBuilder()
                    .WithTitle("🛡️ [고객 센터] 문의 답변")
                    .AddField("📝 내 원본 문의", cacheData.OriginalQuestion, false)
                    .AddField("💡 관리자 답변", cacheData.AdminAnswer, false)
                    .AddField("⏱️ 답변 확인 시각", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), false)
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp()
                    .Build();

                // 1. 에페머럴 응답 전달
                await component.RespondAsync(embed: replyEmbed, ephemeral: true);

                // 2. 답변 확인 후 원본 버튼 안내 메시지 삭제 및 캐시 제거
                try
                {
                    await component.Message.DeleteAsync();
                    ReplyCache.Remove(msgId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "버튼 메시지 삭제 중 권한 또는 네트워크 오류가 발생했습니다.");
                }
            }
            else
            {
                await component.RespondAsync("⚠️ 오래되었거나 만료된 답변 정보입니다.", ephemeral: true);
            }
        }
    }
}

// Thread-safe 캐시 클래스 (ConcurrentDictionary 적용)
public static class ReplyCache
{
    private static readonly ConcurrentDictionary<ulong, (ulong TargetUserId, string OriginalQuestion, string AdminAnswer)> _cache = new();

    public static void Store(ulong messageId, ulong targetUserId, string originalQuestion, string adminAnswer)
    {
        _cache[messageId] = (targetUserId, originalQuestion, adminAnswer);
    }

    public static bool TryGet(ulong messageId, out (ulong TargetUserId, string OriginalQuestion, string AdminAnswer) data)
    {
        return _cache.TryGetValue(messageId, out data);
    }

    public static void Remove(ulong messageId)
    {
        _cache.TryRemove(messageId, out _);
    }
}