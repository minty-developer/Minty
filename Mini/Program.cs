using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args
});

// 2. 파일 감시(reloadOnChange: false)를 완전히 끈 상태로 설정 로드
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 2. Discord.Net 클라이언트 설정 및 DI 등록
builder.Services.AddSingleton(options => new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
    LogLevel = LogSeverity.Info
});
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton(provider => 
    new InteractionService(provider.GetRequiredService<DiscordSocketClient>()));

// 3. 봇을 백그라운드에서 실행할 HostedService 등록 (직접 구현 필요)
builder.Services.AddHostedService<BotWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();