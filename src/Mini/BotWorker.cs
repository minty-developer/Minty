using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class BotWorker : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BotWorker> _logger;
    private readonly EventHandler _eventHandler;

    private bool _commandsRegistered;

    public BotWorker(
        DiscordSocketClient client,
        InteractionService interactions,
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<BotWorker> logger,
        EventHandler eventHandler)
    {
        _client = client;
        _interactions = interactions;
        _services = services;
        _configuration = configuration;
        _logger = logger;
        _eventHandler = eventHandler;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        // Event Handlers
        _client.Log += LogAsync;
        _interactions.Log += LogAsync;

        _client.Ready += ReadyAsync;
        _client.InteractionCreated += HandleInteractionAsync;

        // Users
        _client.UserJoined += _eventHandler.HandleUserJoined;
        _client.UserLeft += _eventHandler.HandleUserLeft;

        // Message
        _client.MessageReceived += _eventHandler.HandleMessageReceived;
        _client.MessageUpdated += _eventHandler.HandleMessageUpdated;
        _client.MessageDeleted += _eventHandler.HandleMessageDeleted;

        // Reaction
        _client.ReactionAdded += _eventHandler.HandleReactionAdded;
        _client.ReactionRemoved += _eventHandler.HandleReactionRemoved;

        // Channel
        _client.ChannelCreated += _eventHandler.HandleChannelCreated;
        _client.ChannelDestroyed += _eventHandler.HandleChannelDestroyed;
        _client.ChannelUpdated += _eventHandler.HandleChannelUpdated;

        // Role
        _client.RoleCreated += _eventHandler.HandleRoleCreated;
        _client.RoleDeleted += _eventHandler.HandleRoleDeleted;
        _client.RoleUpdated += _eventHandler.HandleRoleUpdated;

        // Load Interaction Modules
        var assembly = Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException(
                "Entry Assembly could not be found.");

        await _interactions.AddModulesAsync(
            assembly,
            _services);

        _logger.LogInformation(
            "Interaction modules loaded.");

        // Load Discord Token
        var token =
            Environment.GetEnvironmentVariable("DISCORD_TOKEN")
            ?? _configuration["Discord:Token"];

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Discord Bot Token is not configured.");
        }

        // Connect
        _logger.LogInformation(
            "Connecting to Discord...");

        await _client.LoginAsync(
            TokenType.Bot,
            token);

        await _client.StartAsync();

        _logger.LogInformation(
            "Discord connection started.");

        // Keep Worker Alive
        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown.
        }
    }

    private Task LogAsync(LogMessage message)
    {
        var level = message.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        _logger.Log(
            level,
            message.Exception,
            "[Discord] {Message}",
            message.Message);

        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        _logger.LogInformation(
            "Connected as {Username} ({Id})",
            _client.CurrentUser.Username,
            _client.CurrentUser.Id);

        if (_commandsRegistered)
            return;

        try
        {
            await _interactions.RegisterCommandsGloballyAsync();

            _commandsRegistered = true;

            _logger.LogInformation(
                "Global commands registered.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to register global commands.");
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);

            var result = await _interactions.ExecuteCommandAsync(
                context,
                _services
            );

            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "Interaction failed: {Error}",
                    result.ErrorReason
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Interaction execution failed.");

            if (!interaction.HasResponded)
            {
                await interaction.RespondAsync(
                    "명령어 실행 중 오류가 발생했습니다.",
                    ephemeral: true
                );
            }
        }
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Stopping Discord Bot...");

        _client.Ready -= ReadyAsync;
        _client.InteractionCreated -= HandleInteractionAsync;

        // Users
        _client.UserJoined -= _eventHandler.HandleUserJoined;
        _client.UserLeft -= _eventHandler.HandleUserLeft;

        // Message
        _client.MessageReceived -= _eventHandler.HandleMessageReceived;
        _client.MessageUpdated -= _eventHandler.HandleMessageUpdated;
        _client.MessageDeleted -= _eventHandler.HandleMessageDeleted;

        // Reaction
        _client.ReactionAdded -= _eventHandler.HandleReactionAdded;
        _client.ReactionRemoved -= _eventHandler.HandleReactionRemoved;

        // Channel
        _client.ChannelCreated -= _eventHandler.HandleChannelCreated;
        _client.ChannelDestroyed -= _eventHandler.HandleChannelDestroyed;
        _client.ChannelUpdated -= _eventHandler.HandleChannelUpdated;

        // Role
        _client.RoleCreated -= _eventHandler.HandleRoleCreated;
        _client.RoleDeleted -= _eventHandler.HandleRoleDeleted;
        _client.RoleUpdated -= _eventHandler.HandleRoleUpdated;

        _client.Log -= LogAsync;
        _interactions.Log -= LogAsync;

        await _client.StopAsync();
        await _client.LogoutAsync();

        await base.StopAsync(cancellationToken);

        _logger.LogInformation(
            "Discord Bot stopped.");
    }
}