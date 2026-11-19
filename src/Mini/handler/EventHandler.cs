using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

public class EventHandler
{
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(ILogger<EventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleMessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot)
            return Task.CompletedTask;

        _logger.LogInformation(
            "Message received: {Username} ({UserId}) in {ChannelId}",
            message.Author.Username,
            message.Author.Id,
            message.Channel.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleMessageUpdated(
        Cacheable<IMessage, ulong> before,
        SocketMessage after,
        ISocketMessageChannel channel)
    {
        if (after.Author.IsBot)
            return Task.CompletedTask;

        string previousContent = before.HasValue
            ? before.Value.Content
            : "(캐시에 없음)";

        _logger.LogInformation(
            "Message updated: {MessageId} | Channel: {ChannelId}",
            after.Id,
            channel.Id
        );

        if (before.HasValue && previousContent != after.Content)
        {
            _logger.LogInformation("Message content was changed.");
        }

        return Task.CompletedTask;
    }

    public Task HandleMessageDeleted(
        Cacheable<IMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel)
    {
        if (message.HasValue && message.Value.Author.IsBot)
            return Task.CompletedTask;

        _logger.LogInformation(
            "Message deleted: {MessageId} | Channel: {ChannelId} | Cached: {Cached}",
            message.Id,
            channel.Id,
            message.HasValue
        );

        return Task.CompletedTask;
    }

    public Task HandleUserJoined(SocketGuildUser user)
    {
        _logger.LogInformation(
            "User joined: {Username} ({UserId}) | Guild: {GuildId}",
            user.Username,
            user.Id,
            user.Guild.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleUserLeft(SocketGuild guild, SocketUser user)
    {
        _logger.LogInformation(
            "User left: {Username} ({UserId}) | Guild: {GuildId}",
            user.Username,
            user.Id,
            guild.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleReactionAdded(
        Cacheable<IUserMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction)
    {
        if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            return Task.CompletedTask;

        _logger.LogInformation(
            "Reaction added: {Emoji} | User: {UserId} | Message: {MessageId} | Channel: {ChannelId}",
            reaction.Emote,
            reaction.UserId,
            message.Id,
            channel.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleReactionRemoved(
        Cacheable<IUserMessage, ulong> message,
        Cacheable<IMessageChannel, ulong> channel,
        SocketReaction reaction)
    {
        if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            return Task.CompletedTask;

        _logger.LogInformation(
            "Reaction removed: {Emoji} | User: {UserId} | Message: {MessageId} | Channel: {ChannelId}",
            reaction.Emote,
            reaction.UserId,
            message.Id,
            channel.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleChannelCreated(SocketChannel channel)
    {
        if (channel is not SocketGuildChannel guildChannel)
            return Task.CompletedTask;

        _logger.LogInformation(
            """
            Channel created:
            Name: {ChannelName}
            ID: {ChannelId}
            Type: {ChannelType}
            Guild: {GuildName} ({GuildId})
            """,
            guildChannel.Name,
            guildChannel.Id,
            guildChannel.GetType().Name,
            guildChannel.Guild.Name,
            guildChannel.Guild.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleChannelDestroyed(SocketChannel channel)
    {
        if (channel is not SocketGuildChannel guildChannel)
            return Task.CompletedTask;

        _logger.LogInformation(
            """
            Channel destroyed:
            Name: {ChannelName}
            ID: {ChannelId}
            Type: {ChannelType}
            Guild: {GuildName} ({GuildId})
            """,
            guildChannel.Name,
            guildChannel.Id,
            guildChannel.GetType().Name,
            guildChannel.Guild.Name,
            guildChannel.Guild.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleChannelUpdated(
        SocketChannel before,
        SocketChannel after)
    {
        if (before is not SocketGuildChannel oldChannel ||
            after is not SocketGuildChannel newChannel)
            return Task.CompletedTask;

        _logger.LogInformation(
            """
            Channel updated:
            Channel ID: {ChannelId}
            Guild ID: {GuildId}
            Name: {OldName} -> {NewName}
            """,
            newChannel.Id,
            newChannel.Guild.Id,
            oldChannel.Name,
            newChannel.Name
        );

        return Task.CompletedTask;
    }

    public Task HandleRoleCreated(SocketRole role)
    {
        _logger.LogInformation(
            """
            Role created:
            Name: {RoleName}
            ID: {RoleId}
            Color: {RoleColor}
            Permissions: {Permissions}
            Position: {Position}
            Guild: {GuildName} ({GuildId})
            """,
            role.Name,
            role.Id,
            role.Colors,
            role.Permissions,
            role.Position,
            role.Guild.Name,
            role.Guild.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleRoleDeleted(SocketRole role)
    {
        _logger.LogInformation(
            """
            Role deleted:
            Name: {RoleName}
            ID: {RoleId}
            Color: {RoleColor}
            Permissions: {Permissions}
            Position: {Position}
            Guild: {GuildName} ({GuildId})
            """,
            role.Name,
            role.Id,
            role.Colors,
            role.Permissions,
            role.Position,
            role.Guild.Name,
            role.Guild.Id
        );

        return Task.CompletedTask;
    }

    public Task HandleRoleUpdated(SocketRole before, SocketRole after)
    {
        _logger.LogInformation(
            """
            Role updated:
            Role ID: {RoleId}
            Guild ID: {GuildId}

            Name: {OldName} -> {NewName}
            Color: {OldColor} -> {NewColor}
            Permissions: {OldPermissions} -> {NewPermissions}
            Position: {OldPosition} -> {NewPosition}
            Hoist: {OldHoist} -> {NewHoist}
            Mentionable: {OldMentionable} -> {NewMentionable}
            """,
            after.Id,
            after.Guild.Id,
            before.Name,
            after.Name,
            before.Colors,
            after.Colors,
            before.Permissions,
            after.Permissions,
            before.Position,
            after.Position,
            before.IsHoisted,
            after.IsHoisted,
            before.IsMentionable,
            after.IsMentionable
        );

        return Task.CompletedTask;
    }
}