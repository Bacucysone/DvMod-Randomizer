# Archipelago Setup (outdated)
This document contains some information about the setup of Derail Valley for Archipelago. It is best completed with the Archipelago official docs.

## Before playing the game
### Creating the options yaml file
The options file contains all of what you (the player) want to have in the game. Here is the list of all the possible options and some explanations if needed.
___
#### End goal 
- **victory_condition**: There are currently 2 victory conditions
  -  `number_of_jobs` = Finish enough jobs in enough stations
  -  `demo_loco` = Finish restoring enough demonstrator locomotives
- **nb_jobs**: _Only relevant if you chose `number_of_jobs` as victory condition._ Number of completed jobs to finish one station.
- **nb_stations**: _Only relevant if you chose `number_of_jobs` as victory condition._ Number of stations to finish to win the game.
- **nb_demo_locos**: _Only relevant if you chose `demo_loco` as victory_condition._ Number of demonstrator locomotives to restore to win the game.
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

- **Hints for loco license**: Choose whether the item unlocked for a given number of jobs with a loco appear on the corresponding license

- **Hints for station license**: Choose whether the items unlocked for doing jobs in a given station appear on the corresponding license

- **Death Link**: In a multiworld, you can choose to link the death of the players (meaning that if one player dies in its game, all the players who activated death link will also die). In Derail Valley, that consists in derailing. This option does nothing you are the only one who activated deathlink in a multiworld.

- **Double tokens**: The proportion of double job tokens that will be put in the item pool. It ranges from 0 (no tokens) to 100 (all filler items will be tokens).

___
### Setup the multiworld
When all the yaml files of the players are ready, one player has to generate the multiworld. To do so, if a Derail Valley yaml is concerned, you will need the .apworld file (available in the relases section).
### Setup Derail Valley
To play Archipelago Derail Valley, you will need to mod the game. The only requirement is Unity Mod Manager, the randomizer is self-sufficient. You just have to download the zipfile of the mod in the releases section and install it using UMM. It has not been tested with other mods so I do not know about the compatibility. I believe that QoL mods that do not change the game features should work, but other mods may have funny behaviour with this one. Finally, I should have guarded all functionalities of the mod if the game detected is a vanilla game, but if you want to be sure that your other saves are untouched, **either backup your saves or disable the randomizer mod before launching another save!!**. As an additional measure, it is recommended to entirely quit the game when leaving a randomizer session if you want to load a vanilla save.
### After the multiworld generation
When the multiworld is generated, the game is ready. 
## During the game
### Hosting the multiworld
Derail Valley needs nothing in particular
### Derail Valley difficulty
You can play with any difficulty settings as you want. It is strongly recommended to put the station teleportation at least on (either free or not), as when going from station A to station B for a job, you may not have the license for station B so you may want to teleport somewhere else (as station A for example). This is not required as you can always take jobs from station B and still earn the money rewards, you will not earn Archipelago items and Victory progression though.
### Starting the game
When the game starts, the mod panel opens. You will need to set the information of your multiserver in the randomizer option panel: address of the server (usually localhost if you host locally, or archipelago.gg), the port, your slot name (the one you put in the option yaml) and your password if you chose any. The first time, you will also have to press the "Create a new AP Save on New Career". You will then need to start a new career, you can choose the name and the difficulty, and you must skip tutorial. To prevent progress not correctly recorded, if the game cannot connect to the Archipelago server, the loading will stop and you will go back to main menu. If you manage to load into the game and start playing, that should mean that everything is set correctly. The option to Create a new AP Save will automatically be turned off when the first loading is successful, so you will have to check it again if you want to play another game.
### While playing
Everything, between checking locations and getting items should be automatic through the mod, you just have to play. To enter the different commands of Archipelago (!hint, !collect, !release, etc...) you can either connect through a text client, or use the integrated developer terminal of Derail Valley (default hotkey `\``)
### Continuing a save
To continue a save, you just have to continue the save you have created the first time. Server information will be saved in the mod settings panel, so if you are playing only one archipelago game, you do not need to change credentials (server address, port, slot name and password). When spawning in the game, you will then receive all the items that were sent to you when you were not playing, and you can resume playing as usual.