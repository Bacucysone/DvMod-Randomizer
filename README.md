# Derail Valley Randomizer
This is the main repository for an Archipelago implementation for the game Derail Valley.
Here is the source code for the APWorld (logic of the randomizer, in python, using the Archipelago structure) and the mod for Derail Valley (Installed using Unity mod manager, installation setup in another file TODO). The needed files to play are (will be) in the release section.

## Features
You can play solo, or with other players (who can play on any game supported by Archipelago/Multiworld). The Archipelago part is provided as an .apworld file, please consult the Archipelago docs for further instructions.
- The two main components of an archipelago playthrough are "items" and "locations". Locations are what you have to do/where you have to go to unlock things, and items are what you can unlock. The full list of locations and items of Derail Valley are (will be) somewhere.
- Some locations need some items to be checked. A logic ensures that every location can eventually be checked (If location A needs item B to be checked, it is impossible for location A to unlock item B, it will unlock something else).
- You can either play a single game of randomized Derail Valley, or create a multiworld with other games (even other copies of Derail Valley), played by as many players as you want. If there are several games in your multiworld, you may unlock items from other games in Derail Valley, and the items you need to progress in Derail Valley may be located in other games.
- To generate a playthrough, you need to provide Archipelago with an options files (called yaml). This file contains several settings (a list can be -- will be -- found somewhere). With these settings, you can tailor the game as you want, do not hesitate to send feedbacks about what you want to see.

## Future plans
I have several things I want to do, in no particular order
- Randomizing the shops (Items in shops will not be what they are in the original game, instead they will unlock a random item)
- Improve the overall balance of the game (Museum license is extremely important in terms of numbers of checks, while other progression items such as the garage keys unlock very few checks). This may come with a re-do of the logic (Smooth out the playing experience, especially with regards to money making and money-based locations, maybe adding uses to otherwise less useful parts of the game, such as the caboose, the player house, maybe combining other mods (passenger jobs, custom cars, multiplayer, etc...)).
- For now, the developer terminal console of the game is used as an interface with the server (you need to use it to send commands such as !hint, or you may connect using a text client). Ultimately, I want to remove the use of the terminal in favor of in-game elements (the career manager, the pieces of paper that are everywhere...)
- This version is very much a beta version, almost proof of concept. There are lots of code hacks (regarding network communication for example) that would benefit from a re-factor, but have no impact on the game content.

## Licensing and contributions
This code is under the MIT license (basically, you can do whatever you want with this as long as you ship the license file with your modified version of the code).
Propositions are very much welcome, either as code modification (pull request) or as simple suggestions.
You can contact me on this github (hopefully...) or on discord (join either the Derail Valley or the Archipelago server to find me)

Original Archipelago repository (basic framework, no modification apart from the addition of derail valley world)
    Archipelago multiclient for .NET (Modified version, modifications are available on this repository)
    Websocket csharp (Workaround for easier network communication, modified version)

I took inspiration from many different projects for different parts of the randomizer, including many existing Derail Valley mods (Passenger jobs, custom car loaders, Steam/Shunting start, to cite a few) and many Archipelago worlds (Outer wilds, Rogue Legacy, Castlevania DoS). Have fun!