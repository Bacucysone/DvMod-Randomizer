using System.Collections.Generic;
using System.Drawing.Text;
using DV;
using DV.Logic.Job;
using DV.ServicePenalty.UI;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.UI;
using DV.Utils;
using HarmonyLib;
using UnityEngine;

namespace DvMod.Randomizer {
    [HarmonyPatch(typeof(StationLocoSpawner), nameof(StationLocoSpawner.Update))]
    public static class StationLocoSpawnPatch {
        private static bool RefreshLocos = false;
        public static void DoRefresh() {
            RefreshLocos = true;
        }
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
        [HarmonyPostfix]
        public static void RefreshPatch(StationLocoSpawner __instance) {
            if (Main.player == null) return;
            if(__instance.playerEnteredLocoSpawnRange && RefreshLocos) {
                RefreshLocos = false;
                List<TrainCarLivery> newLoco = GetRandomLicensedLoco();
                SingletonBehaviour<CarSpawner>.Instance.DeleteTrainCarsFromTrack(__instance.locoSpawnTrack);
                SingletonBehaviour<CarSpawner>.Instance.SpawnCarTypesOnTrack(newLoco, null, __instance.locoSpawnTrack, true, true, 0.0, __instance.spawnRotationFlipped);
            }
        }
    }

    [HarmonyPatch(typeof(SleepingUIController), "OnConfirmSleepClicked")]
    public static class SleepPatcher {
        
        public static void Prefix() {
            if (Main.player == null) return;
            StationController? NearestController = StationController.allStations.FindMin(cont => (PlayerManager.PlayerTransform.position - cont.transform.position).magnitude);
            NearestController?.RegenerateJobs();
            StationLocoSpawnPatch.DoRefresh();
        }
    }
}