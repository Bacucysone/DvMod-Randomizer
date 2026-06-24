using HarmonyLib;
using UnityEngine;

namespace DvMod.Randomizer.HarmonyPatches;

/// <summary>
/// Change the default spawn point to be the station mentioned in the slot data
/// </summary>
public class LevelInfoPatch {
    
    [HarmonyPostfix, HarmonyPatch(nameof(LevelInfo.NewCareerSpawnPosition), MethodType.Getter)]
    public static void NewCareerSpawnPositionGet_Postfix(ref Vector3 __result) => __result = 
        StationController.allStations
            .Find(sc => sc.stationInfo.YardID.Equals(Main.Player.SlotData.StartStation))
            .stationRange
            .transform
            .position;
}