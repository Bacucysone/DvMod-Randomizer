# Archipelago Setup
This document contains some information about the setup of Derail Valley for Archipelago. It is best completed with the Archipelago official docs.

## Before playing the game
### Creating the options yaml file
The options file contains all of what you (the player) want to have in the game. Here is the list of all the possible options and some explanations if needed.
___
- **Dispatcher license**: 3 possibilities
 - From Start: you start with the dispatcher license in your starting items
 - Non Randomized: the dispatcher license will be at its vanilla location (can be bought for $10000 in the career manager with no pre-requisites)
 - Randomized: The dispatcher license can be put anywhere (**WARNING** The dispatcher license is rated useful and not progression. Choosing this and you may have to play without it for the majority of your playthrough)

- **Starting money**: How much money you start the game with. Has technically no range limit, you can even put 0

- **Station Licenses**: The new addition of the randomizer
 - No licenses: Removes the need of Station licenses (if you prefer a less altered gameplay), basically gives you the 20 licenses as starting items.
 - Start with one: Gives you one at random (excluding the Military Base) at the start of the game. You will also spawn in this station.
 - All random: No restrictions on the placement of the station (**WARNING** if you have 0 licenses, you cannot progress the game. Choose this only if there are other games in your multiworld, and you are okay with having to wait to be unlocked by other games)

- **Start Loco License**:
 - You can choose either of the 6 locomotives and start with its license
 - Starting random: You start at random with either a DE2, a DM3 or a S060 license (you don't know which one). 
For both of these choices, if the starting loco is a steam one (S060 or S282), you will also start with a regular shovel, an oiler and a lighter
 - Full random: No restriction (**WARNING** Again, this means that you may be locked at the start and that you have to wait for another player to find one of your licenses in their game)

- **Number of shunting jobs**: It can range from 1 to 256. No large scale test has been done yet, but I think that 2-3 is a good number for a short session, 7-8 for a longer one, and more for a span of several weeks

- **Number of transport jobs**: Same as above, but for freight haul and logistical haul. Thanks to concurrent jobs, I think the numbers can be bumped up (4-5 for a short session and around 10 for a long one)

- **Number of jobs required per loco**: You choose the number of jobs that you have to finish with each loco type for a check (this means 6 checks). *note: why is this different than the other? This is purely a design choice. I figured that having 2 checks everytime you finish a job for your first few travels was not satisfactory. But this can easily be changed, or added as a choice in the future*

- **Starting loco license**: To work you also need the license from one loco
 - You can pick either one the three available licenses
 - You can choose to get one random (you don't know which)
 - Or you can choose to not restrict the placement **WARNING** It is still the same warning, with no starting job license, you will need to find one in another game before playing Derail Valley

- **Number of finished stations required to beat the game**: As the name implies, to win the game, you need to finish a set number of stations, that you can specify here. There are 20 stations in the game so you can choose any number between 1 and 20. If you choose another number than 20, the stations that you have to finish are not set. For example, if you chose 15, as soon as you complete your first 15 stations, whatever they are, you will win.

- **Number of jobs required to finish a station**: The other part of the victory condition: "Finishing a station" means performing a sufficient number of jobs (either haul or shunt).
___
### Setup the multiworld
When all the yaml files of the players are ready, one player has to generate the multiworld. To do so, if a Derail Valley yaml is concerned, you will need the .apworld file (available in the relases section).
### Setup Derail Valley
To play Archipelago Derail Valley, you will need to mod the game. The only requirement is Unity Mod Manager, the randomizer is self-sufficient. You just have to download the zipfile of the mod in the releases section and install it using UMM. It has not been tested with other mods so I do not know about the compatibility. I believe that QoL mods that do not change the game features should work, but other mods may have funny behaviour with this one. Finally, I should have guarded all functionalities of the mod if the game detected is a vanilla game, but if you want to be sure that your other saves are untouched, **either backup your saves or disable the randomizer mod before launching another save!!**. As an additional measure, it is recommended to entirely quit the game when leaving a randomizer session if you want to load a vanilla save.
### After the multiworld generation
When the multiworld is generated, the game is ready. A file is generated for Derail Valley in the multiworld zip. **As for now, the only way to obtain it is to manually unzip the archive**. The file has a .save extension, it is a simple savefile for Derail Valley. You will need to put this file in the Imported Saves folder of Derail Valley (you can either locate it on your machine, or use the main menu button that brings you there).
## During the game
### Hosting the multiworld
Derail Valley needs nothing in particular
### Starting the game
When the game starts, the mod panel opens. You will need to set the informations of your multiserver in the randomizer option panel: address of the server (usually localhost if you host locally, or archipelago.gg), the port, your slot name (the one you put in the option yaml) and your password if you chose any. You can then start the imported save.
### While playing
Everything, between checking locations and getting items should be automatic through the mod, you just have to play. To enter the different commands of Archipelago (!hint, !collect, !release, etc...) you can either connect through a text client, or use the integrated developer terminal of Derail Valley (default hotkey `\``)