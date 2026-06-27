using System.Linq;
using DV.Teleporters;
using HarmonyLib;
using UnityEngine;

namespace DvMod.Randomizer.HarmonyPatches;

/// <summary>
/// Change the default spawn point to be the station mentioned in the slot data
/// </summary>
[HarmonyPatch(typeof(LevelInfo))]
public class LevelInfoPatch {
    
    [HarmonyPostfix, HarmonyPatch(nameof(LevelInfo.NewCareerSpawnPosition), MethodType.Getter)]
    public static void NewCareerSpawnPositionGet_Postfix(ref Vector3 __result) => __result = 
        FastTravelDestination.ActiveDestinations
            .OfType<StationFastTravelDestination>()
            .First(sDest => sDest.StationController.stationInfo.YardID == Main.Player.SlotData.StartStation)
            .playerTeleportAnchor
            .position;
}