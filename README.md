# SysBot.NET
![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)

## Support Discords:

I will happily answer any question in my Discord Server I am not like the other creators who get upset and ban people for asking questions. I will not be rude or treat you like you are stupid. The bot community is made up of rude and obnoxious people who treat you like you are a burden. I share all my code I keep nothing secret and I will always publish updates as soon as they are available. When I have pointers I'll post them instead of holding onto it long enough that all the new content in the game is no longer new so I don't have any competition. Lastly use this code to do whatever you like I don't care if you use to make money or anything else that other people would judge you for make your bag this world is to tough to be judging people for doing what they have to do to survive.

If you would like to contribute to this project or have ideas that I may not have thought of join my server and share them if you want I'd be happy to hear them. 

For specific support for this fork of ForkBot's fork of kwsch's SysBot.NET repo feel free to join! (No support will be provided in the official PKHeX or PA Discord, please don't bother the devs)

[Support Server: AllieandJoey](https://discord.gg/3dXQ3q6sdv)


[Base Code Creator Discord Server: Rosé Garden](https://discord.gg/G23Mx85Mdz)

[USB-Botbase](https://github.com/Koi-3088/USB-Botbase) client for remote USB control for this fork.

[sys-botbase](https://github.com/olliz0r/sys-botbase) client for remote control automation of Nintendo Switch consoles.

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

Uses [Discord.Net](https://github.com/discord-net/Discord.Net) , [TwitchLib](https://github.com/TwitchLib/TwitchLib) and [StreamingClientLibary](https://github.com/SaviorXTanren/StreamingClientLibrary) as a dependency via Nuget.

## Other Dependencies
Pokémon API logic is provided by [PKHeX](https://github.com/kwsch/PKHeX/), and template generation is provided by [AutoMod](https://github.com/architdate/PKHeX-Plugins/).

Permutation generation provided by [PermuteMMO](https://github.com/kwsch/PermuteMMO).
# License
Refer to the `License.md` for details regarding licensing.
