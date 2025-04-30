using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus;

namespace MyDiscordBot.commands
{
    public class TestCommands : BaseCommandModule

    {
        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(content: "просто напиши "/" и ты увидишь все комманды с описанием =)");
        }
        
        
        [Command(name: "hello")]
        public async Task Hello(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("привет");
        }

        
        [Command(name: "random")] // комманда рандмных числел. пример: !random 1 100
        public async Task Random(CommandContext ctx, int min, int max)
        {
            var randomValue = new System.Random().Next(min, max);
            await ctx.Channel.SendMessageAsync( content:ctx.User.Mention + " - ваше число " + randomValue);
        }
        
        private const ulong TargetUserId = 1234567890123456789; // всавьте сюда любое айди кого хотите чтобы отпраляло в тайм-аут на 1 минуту.

        [Command("!")]
        public async Task TimeoutUser(CommandContext ctx)
        {
            if (ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                var guild = ctx.Guild;
                var member = await guild.GetMemberAsync(TargetUserId);
                
                if (member != null)
                {
                    var duration = TimeSpan.FromMinutes(1);
                    await member.TimeoutAsync(DateTime.UtcNow + duration);
                    await ctx.RespondAsync($"{member.Username} был отправлен в тайм-аут на 1 минуту.");
                }
                else
                {
                    await ctx.RespondAsync("Пользователь не найден.");
                }
            }
            else
            {
                await ctx.RespondAsync("У вас нет прав.");
            }
        }
        
    }
}
