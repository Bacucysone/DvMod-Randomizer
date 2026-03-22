# Locations and items of Derail Valley
Here is the complete list of locations and items possible in this version of AP Derail Valley. The actual list of locations and items in your playthrough depends on the options you have chosen in your yaml
## Victory condition
Every Archipelago framework need a victory condition. For Derail Valley, you have to finish a specified number of station, and finishing a station means finishing enough jobs in this station. The usefulness of the victory depends on what you want to do, and the configuration of the host during generation. You can choose to activate certain commands (!release or !collect) only when the player has completed their goal. But you can also choose to completely disregard this aspect and just play the game as you wish
## Item list
Items are what you can unlock for Derail Valley. We rate items depending on their usefulness: from lowest to highest usefulness we have traps (detrimental in some ways, only trap in Derail Valley currently is the Nothing item), filler (not useful, used to cover all locations when the most important items are already placed), useful (not required, but nice to have) and progression (unlock locations in some ways). The rate of the items matter for the option "progression_balancing" of the yaml. Consult Archipelago docs for more info.

### Station licenses
A new mechanic added to the randomizer are station licenses. There exists one license for each station (for a total of 20). You need the corresponding license in order to unlock items from jobs and to check the demonstrator locomotives locations (see below). You do not need a station license to take and clear a job, but you will not earn location checks doing so. You will however still earn money. There are 20 station licenses rated progression.

### Game licenses
Each of the game licenses is an item. They are usually progression items, with some exceptions (Dispatcher and Multiple unit are useful, and Train Conductor is filler). For tiered licenses (Concurrent jobs, Train length, Hazmat and Military), we introduce progressive licenses. Instead of a Train Length 1 and a Train Length 2 licenses, there are 2 Progressive Train Length licenses. When you acquire the first one you will get the Train Length 1 license and the second one will get you the Train Length 2.

### Crew vehicles
The right to spawn the four hidden vehicles (BE2, Caboose, DE6 Slug, DM1U) are 4 items, rated filler.

### Relic locomotives
The restoration side quest is reworked. No demonstrator will spawn in the world. Instead, there are 2 Progressive demo locos for each of the locomotive types (for a total of 12 items) hidden in the multiworld. The first Progressive demo loco will spawn the corresponding loco at its place in the museum. When you find the Museum license, you can then purchase the repair parts and bring them back from the Machine Factory (as usual). To progress further, you will need the second Progressive demo loco. It will allow you to purchase the effective reparation, and finish the restoration and the paint work. The order in which you get the museum license and the Progressive demo locos do not matter. All 12 Progressive Demo Locos are rated progression.

### Physical items
Every item of Derail Valley can be obtained. The vast majority are rated filler (Files, Flags, Mounts, Gadgets, Boombox related...), and the other are progression items (The 4 garage keys, a paint sprayer and the tools required to operate a steam locomotive).

### Misc items
The last two items are Nothing rated as trap, and Money ($5000) rated as filler.

## Location list
Locations are what you have to do or where you have to go to unlock things. You will also find the rules of the logic for each location.

### Demo loco spawn points
Each of the 56 possible spawn points for demonstrator locomotives are considered locations. To unlock the corresponding item, you just need to get close to the point. You will need both the Museum license and the corresponding station license. For now, it is recommended to follow a guide as they can be hard to find with no indications. (The outlier one is considered to be in CW for station licenses purposes).

### Working
Everytime you complete a job, you get some advancements. For each station, the first Shunting jobs that you complete will each grant you a check. The number of Shunting jobs that give you a check is configurable in the options yaml (For example, if you choose 6, the first 6 Shunting jobs in each station will give you a check, for a total of 120 locations). The same is true for hauling jobs (either transport or logistical), configurable as well. All these locations are locked behind owning the corresponding station license.

Besides, there is one check for each locomotive type, to finish a number of jobs using this loco type (again, specified in the options yaml) for a total of 6 locations.

### Buying licenses
You will notice some question marks in the career manager. All game licenses have been replaced with random items, meaning that buying any license is a location. The requirements to buy any license are same as the original, and are computed using the items you received. This means that, for example, in order to buy the Military 2 license, you will need to have found at least one Progressive Military license elsewhere.

### Hidden garages
The four hidden garages that normally grant you the rights to spawn crew vehicles are now locations, locked behind their respective key (that you must find as an item).

### Restoration side quest
The progression of the restoration quest are gated behind items, but there 12 locations available in the questline (2 for each locomotive). The first check comes when you bring back the machine parts from MF (you only need the museum license and one corresponding progressive demo loco). The second check is at the end of the line, when you have painted both interior and exterior of the new loco (you will need the museum license, the 2 Progressive demo loco, the corresponding loco license, the paint sprayer as it will not spawn in the atelier, and lots lots of money). These last 6 checks are considered very late game. (Balance: find a way to speed them up?)

 