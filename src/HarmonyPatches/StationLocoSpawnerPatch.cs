using System.Collections.Generic;
using DV;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;
/* 
 * This part allow for respawning the locomotives in a station when sleeping
 * As it is, it destroys more cars than necessary (pretty much every locomotive on station track)
 * Free teleport between station is recommended, as such, it is not so hard to find a desired locomotive
 * This feature is turned off for now
 */

/*
[HarmonyPatch(typeof(StationLocoSpawner))]
public class StationLocoSpawnerPatch {
    private static bool RefreshLocos;
    public static void DoRefresh() =>
        RefreshLocos = true;
    
    private static List<TrainCarLivery> GetRandomLicensedLoco() {
        List<List<TrainCarLivery>> allLiveries = [
            [TrainCarType.LocoShunter.ToV2()],
            [TrainCarType.LocoDM3.ToV2()],
            [TrainCarType.LocoDH4.ToV2()],
            [TrainCarType.LocoDiesel.ToV2()],
            [TrainCarType.LocoS060.ToV2()],
            [TrainCarType.LocoSteamHeavy.ToV2(), TrainCarType.Tender.ToV2()]
        ];
        allLiveries.Shuffle();
        foreach (List<TrainCarLivery> loco in allLiveries) {
            if (SingletonBehaviour<LicenseManager>.Instance.IsLicensedForCar(loco[0]))
                return loco;
        }
        return [TrainCarType.HandCar.ToV2()];
    }
    [HarmonyPostfix, HarmonyPatch(nameof(StationLocoSpawner.Update))]
    public static void Update_Postfix(StationLocoSpawner __instance) {
        if (!Main.IsConnected) return;
        if (!__instance.playerEnteredLocoSpawnRange || !RefreshLocos) return;
        RefreshLocos = false;
        List<TrainCarLivery> newLoco = GetRandomLicensedLoco();
        SingletonBehaviour<CarSpawner>.Instance.DeleteTrainCarsFromTrack(__instance.locoSpawnTrack);
        SingletonBehaviour<CarSpawner>.Instance.SpawnCarTypesOnTrack(newLoco, null, __instance.locoSpawnTrack, true, true, 0.0, __instance.spawnRotationFlipped);
    }
}*/


