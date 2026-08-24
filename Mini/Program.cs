using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

// 1. Linux inotify 제한 회피 (Render 환경 파일 감시자 폴링 전환)
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

// 2. WebApplication 빌더 생성
var builder = WebApplication.CreateBuilder(args);

// 3. 파일 감시(reloadOnChange: false) 비활성화 설정
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// 4. 서비스 등록 (OpenAPI & Controllers)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 5. Discord.Net 클라이언트 설정 및 DI 등록
builder.Services.AddSingleton(options => new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
    LogLevel = LogSeverity.Info
});
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton(provider => 
    new InteractionService(provider.GetRequiredService<DiscordSocketClient>()));

// 6. 백그라운드 봇 서비스 등록
builder.Services.AddHostedService<BotWorker>();

var app = builder.Build();

// 7. 파이프라인 구성
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();