using Discord;
using Discord.Interactions;

namespace discord_template
{
    internal class SlashCommandModule : InteractionModuleBase<IInteractionContext>
    {
        [SlashCommand("test", "testcommand")]
        public async Task TestCmd(string input)
        {
            await RespondAsync(input);
        }
    }
}
