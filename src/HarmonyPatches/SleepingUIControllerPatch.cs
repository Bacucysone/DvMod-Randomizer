using DV.UI;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(SleepingUIController))]
public class SleepingUIControllerPatch {
    /// <summary>
    /// The randomizer might force you to only do shunting jobs in a small station. To make it easier, sleeping forces a job regeneration in the current station
    /// </summary>
    [HarmonyPrefix, HarmonyPatch("OnConfirmSleepClicked")]
    public static void OnConfirmSleepClicked_Prefix() {
        if (!Main.IsConnected) return;
        StationController nearestController = StationController.allStations.FindMin(cont => (PlayerManager.PlayerTransform.position - cont.transform.position).magnitude);
        nearestController?.RegenerateJobs();
        // StationLocoSpawnerPatch.DoRefresh();
    }
}