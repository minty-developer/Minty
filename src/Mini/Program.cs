using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    GatewayIntents =
        GatewayIntents.Guilds |
        GatewayIntents.GuildMembers |
        GatewayIntents.GuildMessages |
        GatewayIntents.DirectMessages |
        GatewayIntents.GuildMessageReactions |
        GatewayIntents.DirectMessageReactions |
        GatewayIntents.MessageContent,

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

// Event Handler
builder.Services.AddSingleton<EventHandler>();

// Background Services
builder.Services.AddHostedService<BotWorker>();

// Build
var host = builder.Build();

// Run
await host.RunAsync();