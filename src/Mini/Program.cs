using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DotNetEnv;

// Environment Variables
Env.Load();

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Discord Configuration
builder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds
                   | GatewayIntents.GuildMessages
                   | GatewayIntents.MessageContent,

    LogLevel = LogSeverity.Info,
    MessageCacheSize = 50
});

// Discord Client
builder.Services.AddSingleton<DiscordSocketClient>();

// Interaction Service
builder.Services.AddSingleton(provider =>
    new InteractionService(
        provider.GetRequiredService<DiscordSocketClient>()
    )
);

// Background Services
builder.Services.AddHostedService<BotWorker>();

// Build
var host = builder.Build();

// Run
await host.RunAsync();