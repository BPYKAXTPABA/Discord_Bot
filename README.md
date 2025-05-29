# 🛡️ Discord Moderation Bot

**Alfredo_2033** is a powerful and customizable Discord moderation bot built using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).  
It is designed to help manage communities by providing essential moderation tools such as warnings, mutes, bans, and user status checks. Ideal for small to mid-sized servers.

## ✨ Features

- `/warn` — Issue a warning to a user  
- `/unwarn` — Remove a warning  
- `/checkwarns` — View a user's warning count  
- `/mutechat` / `/unmutechat` — Mute/unmute users in text channels  
- `/mutevoice` / `/unmutevoice` — Mute/unmute users in voice channels  
- `/ban`, `/kick`, `/unban` — User management commands  
- `/rules` — Display server rules  
- `/info` — Display bot info  
- Automatic punishments:
  - 3 warnings = chat mute  
  - 5 warnings = ban

## ⚙️ Requirements

- [.NET Framework 4.8.1](https://dotnet.microsoft.com/en-us/download/dotnet-framework)  
- A Discord Bot Token  
- Roles `MutedFromChat` and `MutedFromVoice` are auto-created on startup  

## 🚀 Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/BPYKAXTPABA/Discord_Bot.git
    ```

2. Open the project in Visual Studio.

3. Install dependencies via NuGet:
    - `DSharpPlus`
    - `Newtonsoft.Json` (for basic in-memory storage)

4. Add your Discord bot token to the config file.

5. Run the bot.

## 🧠 How It Works

Warnings are currently stored in memory only.  
Future updates will include database storage support (SQLite).

## 📝 TODO

- [ ] Add SQLite support  
- [ ] Logging system  
- [ ] Config file with permission settings  
- [ ] Enhanced slash commands with parameters  

## 📄 License

See [LICENSE](LICENSE) for details.
