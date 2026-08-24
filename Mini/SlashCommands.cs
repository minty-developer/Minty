using Discord.Interactions;

public class HelloModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("안녕", "봇과 가볍게 인사를 나눕니다.")]
    public async Task HandleHelloCommand()
    {
        await RespondAsync($"안녕하세요, {Context.User.Mention}! 반가워요!");
    }
}