using HarmonyLib;
using DV;
using DV.ThingTypes;
using DV.Utils;
using System.Linq;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(CommsRadioCrewVehicle))]
    public static class CrewCommsPatch {
        [HarmonyPostfix, HarmonyPatch("UpdateAvailableVehicles")]
        public static void CustomVehicles(CommsRadioCrewVehicle __instance) {
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
        /*[HarmonyPrefix, HarmonyPatch("SetState")]
        public static void RefreshList(CommsRadioCrewVehicle.State newState, CommsRadioCrewVehicle __instance) {
            if (newState == CommsRadioCrewVehicle.State.EnterSpawnMode)
                __instance.UpdateAvailableVehicles();
        }*/
    }

    [HarmonyPatch(typeof(GaragePadlockUnlocker), "OnGarageUnlocked")]
    public static class GaragePatcher {
        public static void Prefix(GarageType_v2 unlockedGarageType) {
            if (!Main.IsConnected) return;
            switch (unlockedGarageType.v1) {
                case Garage.Caboose: Main.Player.UnlockCheck(0x691); break;
                case Garage.DM1U: Main.Player.UnlockCheck(0x693); break;
                case Garage.Bob: Main.Player.UnlockCheck(0x692); break;
                case Garage.DE6_Slug: Main.Player.UnlockCheck(0x690); break;
            }
        }
    }
}