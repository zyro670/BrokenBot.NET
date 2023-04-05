# SysBot.NET
![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)

This is a fork of a fork with some QoL improvements and/or new bots (I mainly focus on Scarlet/Violet).

## Support Discord:

For support on setting up your own instance of SysBot.NET, feel free to check the wiki (which is under construction)

- [VileTung#6635](https://discordapp.com/users/367614335431868417) on Discord, just send me a message
- ~[official Discord support server](https://discord.gg/tDMvSRv)~ (please, beaware this is a fork and you might not get help!)
- [sys-botbase](https://github.com/olliz0r/sys-botbase) client for remote control automation of Nintendo Switch consoles.
- [Hybrid sys-botbase](https://github.com/Koi-3088/sys-usb-botbase) client for remote control using a hybrid connection (USB or WiFi)
- [USB-Botbase](https://github.com/Koi-3088/USB-Botbase) client for remote USB control.

## SysBot.Base:
- Base logic library to be built upon in game-specific projects.
- Contains a synchronous and asynchronous Bot connection class to interact with sys-botbase.

## SysBot.Tests:
- Unit Tests for ensuring logic behaves as intended :)

# Example Implementations

The driving force to develop this project is automated bots for Nintendo Switch Pokémon games. An example implementation is provided in this repo to demonstrate interesting tasks this framework is capable of performing. Refer to the [Wiki](https://github.com/kwsch/SysBot.NET/wiki) for more details on the supported Pokémon features.

## SysBot.Pokemon:
- Class library using SysBot.Base to contain logic related to creating & running Sword/Shield bots.

## SysBot.Pokemon.WinForms:
- Simple GUI Launcher for adding, starting, and stopping Pokémon bots (as described above).
- Configuration of program settings is performed in-app and is saved as a local json file.

## SysBot.Pokemon.Discord:
- Discord interface for remotely interacting with the WinForms GUI.
- Provide a discord login token and the Roles that are allowed to interact with your bots.
- Commands are provided to manage & join the distribution queue.

## SysBot.Pokemon.Twitch:
- Twitch.tv interface for remotely announcing when the distribution starts.
- Provide a Twitch login token, username, and channel for login.

## SysBot.Pokemon.YouTube:
- YouTube.com interface for remotely announcing when the distribution starts.
- Provide a YouTube login ClientID, ClientSecret, and ChannelID for login.

Uses [Discord.Net](https://github.com/discord-net/Discord.Net), [TwitchLib](https://github.com/TwitchLib/TwitchLib) and [StreamingClientLibary](https://github.com/SaviorXTanren/StreamingClientLibrary) as a dependency via Nuget.

## Other Dependencies
Pokémon API logic is provided by [PKHeX](https://github.com/kwsch/PKHeX/), and template generation is provided by [AutoMod](https://github.com/architdate/PKHeX-Plugins/).

# License
Refer to the `License.md` for details regarding licensing.

# Credits
Thanks for all the open source projects. Credits are in no particular order:
- [@LegoFigure11](https://www.github.com/LegoFigure11) for [RaidCrawler](https://github.com/LegoFigure11/RaidCrawler)]
- [@kwsch](https://www.github.com/kwsch) for the base of this bot and [PKHeX](https://github.com/kwsch/PKHeX/)
- [@olliz0r](https://www.github.com/olliz0r) for [sys-botbase](https://github.com/olliz0r/sys-botbase)
- [@Koi-3088](https://www.github.com/Koi-3088) for a fork of the base bot, [Hybrid sys-botbase](https://github.com/Koi-3088/sys-usb-botbase) and [USB-Botbase](https://github.com/Koi-3088/USB-Botbase)
- [@zyro670](https://www.github.com/olliz0r) for [NotForkBot.NET](https://github.com/zyro670/NotForkBot.NET)
- _Everyone else who contributed to the repositories this project uses._
