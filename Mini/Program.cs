using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

// 1. Linux inotify 제한 회피 (Render 환경 파일 감시자 폴링 전환)
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(args);

// 2. 파일 감시(reloadOnChange: false) 비활성화 설정
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// 3. 서비스 등록 (OpenAPI & Controllers)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 4. Discord.Net 클라이언트 설정 및 DI 등록
builder.Services.AddSingleton(provider => new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
    LogLevel = LogSeverity.Info,
    // 컴파일 에러를 일으킨 HandshakeTimeout과 DefaultRetryLimit 제거
    MessageCacheSize = 50,
    ConnectionTimeout = 30000 // DiscordSocketConfig에서 지원하는 연결 타임아웃 설정 (밀리초)
});

builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton(provider => 
    new InteractionService(provider.GetRequiredService<DiscordSocketClient>()));

// 5. 백그라운드 봇 서비스 등록
builder.Services.AddHostedService<BotWorker>();

var app = builder.Build();

// 6. Render Health Check용 기본 루트 엔드포인트 추가
app.MapGet("/", () => "Bot is running!");

// 7. 파이프라인 구성
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();