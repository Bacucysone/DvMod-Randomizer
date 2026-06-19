using System.Linq;
using DV;
using DV.ThingTypes;
using DV.Utils;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(CommsRadioCrewVehicle))]
public static class CommsRadioCrewVehiclePatch {
    
    [HarmonyPostfix, HarmonyPatch("UpdateAvailableVehicles")]
    public static void UpdateAvailableVehicles_Postfix(CommsRadioCrewVehicle __instance) {
        if (!Main.IsConnected) return;
        __instance.availableVehiclesForSpawn.Clear(); 
        __instance.availableVehiclesForSpawn.AddRange( 
            SingletonBehaviour<CarSpawner>.Instance.crewVehicleGarages
                .Where(Main.Player.HasUnlocked)
                .SelectMany(g => g.garageCarLiveries)
                .ToList()
        );
        __instance.availableVehiclesForSpawn.AddRange(SingletonBehaviour<CarSpawner>.Instance.vehiclesWithoutGarage);
    }
}

