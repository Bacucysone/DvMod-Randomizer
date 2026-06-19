using DV.UI;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(SleepingUIController))]
public class SleepingUIControllerPatch {
    
    [HarmonyPrefix, HarmonyPatch("OnConfirmSleepClicked")]
    public static void OnConfirmSleepClicked_Prefix() {
        if (!Main.IsConnected) return;
        StationController nearestController = StationController.allStations.FindMin(cont => (PlayerManager.PlayerTransform.position - cont.transform.position).magnitude);
        nearestController?.RegenerateJobs();
        StationLocoSpawnerPatch.DoRefresh();
    }
}