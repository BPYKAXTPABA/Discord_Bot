using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Linq;
using System.Threading.Tasks;

namespace MyDiscordBot.Commands
{
    public class SlashCommands : ApplicationCommandModule
    {
        private static Dictionary<ulong, int> _warns = new Dictionary<ulong, int>(); // Временная база с варнами пользователей(сбрасывается при перезапуске)

        private bool IsUserAdmin(InteractionContext ctx) // Простая проверка являеться ли пользователь Администратором
        {
            return ctx.Member.Permissions.HasPermission(Permissions.Administrator);
        }

        private bool IsUserModerator(InteractionContext ctx) // Простая проверка являеться ли пользователь модератором
        {
            return ctx.Member.Permissions.HasPermission(Permissions.ModerateMembers);
        }
        
        // 1. Мут в чате
        [SlashCommand("mutechat", "Мут пользователя в чате")]
        public async Task MuteChat(InteractionContext ctx, [Option("user", "Пользователь для мута")] DiscordUser user)
        {
            if (!IsUserModerator(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var muteRole = ctx.Guild.Roles.Values.FirstOrDefault(r => r.Name == "MutedFromChat");

            if (muteRole == null)
            {
                muteRole = await ctx.Guild.CreateRoleAsync("MutedFromChat", Permissions.None, DiscordColor.DarkGray,
                    false, true);
                foreach (var channel in ctx.Guild.Channels.Values)
                {
                    await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages);
                }
            }

            await member.GrantRoleAsync(muteRole);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{member.DisplayName} замучен в чате."));
        }

        // 2. Полный мут в войсе (кик + запрет входа)
        [SlashCommand("mutevoice", "Отключить пользователя из войс-чата и запретить вход")]
        public async Task MuteVoice(InteractionContext ctx,
            [Option("user", "Пользователь для мута в войсе")] DiscordUser user)
        {
            if (!IsUserModerator(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var muteRole = ctx.Guild.Roles.Values.FirstOrDefault(r => r.Name == "MutedFromVoice");

            if (muteRole == null)
            {
                muteRole = await ctx.Guild.CreateRoleAsync("MutedFromVoice", Permissions.None, DiscordColor.DarkGray,
                    false, true);

                foreach (var channel in ctx.Guild.Channels.Values)
                {
                    if (channel.Type == ChannelType.Voice)
                    {
                        await channel.AddOverwriteAsync(muteRole, DSharpPlus.Permissions.None,
                            DSharpPlus.Permissions.UseVoice);
                    }
                }
            }

            await member.GrantRoleAsync(muteRole);

            if (member.VoiceState?.Channel != null)
            {
                await member.ModifyAsync(x => x.VoiceChannel = null);
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(
                    $"{member.DisplayName} замучен в войс-чате и не может подключаться."));
        }

        // 3. Варн пользователя (счётчик + авто-мут/бан)
        [SlashCommand("warn", "Выдать варн пользователю")]
        public async Task Warn(InteractionContext ctx, [Option("user", "Пользователь для варна")] DiscordUser user)
        {
            if (!IsUserAdmin(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            if (!_warns.ContainsKey(user.Id))
                _warns[user.Id] = 0;

            _warns[user.Id]++;

            var member = await ctx.Guild.GetMemberAsync(user.Id);

            if (_warns[user.Id] == 3)
            {
                await MuteChat(ctx, user);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent(
                        $"{member.DisplayName} получил 3 варна и был замучен в чате."));
            }
            else if (_warns[user.Id] >= 5)
            {
                await member.BanAsync();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent(
                        $"{member.DisplayName} получил 5 варнов и был забанен."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent(
                        $"{member.DisplayName} получил варн. Всего варнов: {_warns[user.Id]}/5"));
            }
        }

        // 4. Бан пользователя
        [SlashCommand("ban", "Забанить пользователя")]
        public async Task Ban(InteractionContext ctx, [Option("user", "Пользователь для бана")] DiscordUser user)
        {
            if (!IsUserAdmin(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            await member.BanAsync();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{member.DisplayName} был забанен."));
        }

        // 5. Написание правил
        [SlashCommand("rules", "Вывести правила")]
        public async Task Rules(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Title = "Правила сервера",
                Description =
                    "1. Не флудить\n2. Не оскорблять участников\n3. Не использовать запрещённые слова\n4. Соблюдать правила Discord",
                Color = DiscordColor.Azure
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        // 6. Информация о боте
        [SlashCommand("info", "Вывести информацию о боте")]
        public async Task Info(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Title = "Информация о боте",
                Description = "Обычный модерирующий бот для Discord\nСоздатель бота - alfredo\nDiscord: 1255968122754699305\n Telegram: TPABABPYKAX",
                Color = DiscordColor.Azure
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        // 7. Размут в чате
        [SlashCommand("unmutechat", "Размут пользователя в чате")]
        public async Task UnmuteFromChat(InteractionContext ctx, [Option("user", "Пользователь для размута")] DiscordUser user)
        {
            if (!IsUserModerator(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var muteRole = ctx.Guild.Roles.Values.FirstOrDefault(r => r.Name == "MutedFromChat");
            if (muteRole != null)
            {
                await member.RevokeRoleAsync(muteRole);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"{member.DisplayName} был размучен в чате."));
            }
        }

        // 8. Размут в войсе
        [SlashCommand("unmutevoice", "Размут пользователя в войсе")]
        public async Task UnmuteFromVoice(InteractionContext ctx, [Option("user", "Пользователь для размута")] DiscordUser user)
        {
            if (!IsUserModerator(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var muteRole = ctx.Guild.Roles.Values.FirstOrDefault(r => r.Name == "MutedFromVoice");
            if (muteRole != null)
            {
                await member.RevokeRoleAsync(muteRole);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"{member.DisplayName} был размучен в войсе."));
            }
        }

        // 9. Кик пользователя
        [SlashCommand("kick", "Кикнуть пользователя с сервера")]
        public async Task Kick(InteractionContext ctx, [Option("user", "Пользователь для кика")] DiscordUser user)
        {
            if (!IsUserAdmin(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            await member.RemoveAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"{member.DisplayName} был кикнут с сервера."));
        }

        // 10. Разбан пользователя (только по ID)
        [SlashCommand("unban", "Разбанить пользователя по ID")]
        public async Task Unban(InteractionContext ctx, [Option("userid", "ID пользователя для разбана")] string userIdStr)
        {
            if (!IsUserAdmin(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            if (!ulong.TryParse(userIdStr, out ulong userId))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Ошибка: введён некорректный ID пользователя."));
                return;
            }

            var bannedUsers = await ctx.Guild.GetBansAsync();
            var bannedUser = bannedUsers.FirstOrDefault(b => b.User.Id == userId);

            if (bannedUser != null)
            {
                await ctx.Guild.UnbanMemberAsync(userId);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Пользователь {bannedUser.User.Username} был разбанен."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Пользователь с ID {userId} не найден в списке забаненных."));
            }
        }

        // 11. Снять варн
        [SlashCommand("unwarn", "Снять варн с пользователя")]
        public async Task Unwarn(InteractionContext ctx, [Option("user", "Пользователь для снятия варна")] DiscordUser user)
        {
            if (!IsUserAdmin(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            if (_warns.ContainsKey(user.Id))
            {
                _warns[user.Id]--;
                var member = await ctx.Guild.GetMemberAsync(user.Id);

                // Размут в чате, если количество варнов становится 2
                if (_warns[user.Id] == 2)
                {
                    var muteRole = ctx.Guild.Roles.Values.FirstOrDefault(r => r.Name == "MutedFromChat");
                    if (muteRole != null)
                    {
                        await member.RevokeRoleAsync(muteRole);
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().WithContent($"{member.DisplayName} был размучен в чате и теперь имеет {_warns[user.Id]} варнов."));
                    }
                }

                // Если количество варнов стало 0
                if (_warns[user.Id] <= 0)
                {
                    _warns.Remove(user.Id);
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent($"{user.Username} больше не имеет варнов."));
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().WithContent($"{user.Username} теперь имеет {_warns[user.Id]} варнов."));
                }
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"{user.Username} не имеет варнов."));
            }
        }



        // 12. Получить варны пользователя
        [SlashCommand("checkwarns", "Проверить количество варнов у пользователя")]
        public async Task CheckWarns(InteractionContext ctx, [Option("user", "Пользователь для проверки варнов")] DiscordUser user)
        {
            if (!IsUserAdmin(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            if (_warns.ContainsKey(user.Id))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"{user.Username} имеет {_warns[user.Id]} варнов."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"{user.Username} не имеет варнов."));
            }
        }

        // 13. Список забаненных пользователей
        [SlashCommand("bannedusers", "Показать список забаненных пользователей")]
        public async Task BannedUsers(InteractionContext ctx)
        {
            if (!IsUserAdmin(ctx))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("У тебя нет прав для использования этой команды.")
                        .AsEphemeral(true));
                return;
            }

            var bannedUsers = await ctx.Guild.GetBansAsync();
            var bannedList = string.Join("\n", bannedUsers.Select(b => $"Username - {b.User.Username}  User iD - {b.User.Id}"));

            if (string.IsNullOrEmpty(bannedList))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("На сервере нет забаненных пользователей."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Забаненные пользователи:\n{bannedList}"));
            }
        }
    }
}
