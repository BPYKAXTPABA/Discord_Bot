using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MyDiscordBot.commands.Slash
{
    public class BasicSlashCommands : ApplicationCommandModule

    {

        [SlashCommand("userDetail", "a simple text command with user detail")]
        public async Task TestUserCommand(InteractionContext ctx,
            [Option("user", "get info about selected user")] DiscordUser user)
        {
            await ctx.DeferAsync();

            var member = (DiscordMember)user;

            var embedMessage = new DiscordEmbedBuilder()
            {
                Title = "User Detail",
                Description = $"User: {user.Username}, ID: {user.Id}",
                Color = DiscordColor.Azure
            };
            
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));
        }
    }
}