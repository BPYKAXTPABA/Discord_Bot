using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using MyDiscordBot.commands;
using MyDiscordBot.Commands;
using MyDiscordBot.commands.Slash;
using MyDiscordBot.config;

namespace MyDiscordBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        public static CommandsNextExtension Commands { get; set; } 

        public static async Task Main(string[] args) 
        {
            var jsonReader = new JSONReader();
            await jsonReader.ReadJson();

            var discordConfig = new DiscordConfiguration()
            {
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);
            Client.Ready += ClientOnReady;

            var commandConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new [] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };
            
            var slashCommandsConfig = Client.UseSlashCommands();
            slashCommandsConfig.RegisterCommands<BasicSlashCommands>(); // я тестил комманду, она просто показывает ID и username
            
            slashCommandsConfig.RegisterCommands<SlashCommands>(); // все модерирующие комманды в одном классе

            Commands = Client.UseCommandsNext(commandConfig);
            Commands.RegisterCommands<TestCommands>(); // чисто для себя добавил, кому надо можете удалить

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task ClientOnReady(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}