using HarmonyLib;
using DV;
using System.Collections.Generic;
using DV.ThingTypes;
using DV.Utils;
using CommandTerminal;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(CommsRadioCrewVehicle))]
    public static class CrewCommsPatch {
        [HarmonyPostfix, HarmonyPatch("UpdateAvailableVehicles")]
        public static void CustomVehicles(ref List<TrainCarLivery> ___availableVehiclesForSpawn) {
            if (Main.player == null) 
                return;
            ___availableVehiclesForSpawn.Clear();
            GarageType_v2[] crewVehicleGarages = SingletonBehaviour<CarSpawner>.Instance.crewVehicleGarages;
            foreach (GarageType_v2 garageType_v in crewVehicleGarages) {
                if (Main.player.HasUnlocked(garageType_v)) {
                    foreach (TrainCarLivery livery in garageType_v.garageCarLiveries) {
                        ___availableVehiclesForSpawn.Add(livery);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GaragePadlockUnlocker), "OnGarageUnlocked")]
    public static class GaragePatcher {
        public static void Prefix(GarageType_v2 unlockedGarageType) {
            if (Main.player == null) return;
            switch (unlockedGarageType.v1) {
                case Garage.Caboose: Main.player.UnlockCheck(0x691); break;
                case Garage.DM1U: Main.player.UnlockCheck(0x693); break;
                case Garage.Bob: Main.player.UnlockCheck(0x692); break;
                case Garage.DE6_Slug: Main.player.UnlockCheck(0x690); break;
            }
        }
    }

    [HarmonyPatch(typeof(StationLocoSpawner), nameof(StationLocoSpawner.Awake))]
    public static class LocoSpawnerPatcher {
        public static void Postfix(ref int ___nextLocoGroupSpawnIndex, List<ListTrainCarTypeWrapper> ___locoTypeGroupsToSpawn) {
            if (Main.player==null) return;
            ___nextLocoGroupSpawnIndex = 0;
            while (___locoTypeGroupsToSpawn[___nextLocoGroupSpawnIndex].liveries[0].v1 != TrainCarType.LocoShunter)
            {   
                ___nextLocoGroupSpawnIndex++;
                if (___nextLocoGroupSpawnIndex == ___locoTypeGroupsToSpawn.Count) {
                    Terminal.Log(TerminalLogType.Error, "Couldn't find loco, loading default");
                    ___nextLocoGroupSpawnIndex=0;
                    return;
                }
            }
        }
    }
}