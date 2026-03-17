using DV.UI;
using HarmonyLib;

namespace DvMod.Randomizer {

    [HarmonyPatch(typeof(SleepingUIController), "OnConfirmSleepClicked")]
    public static class SleepPatcher {
        
        public static void Prefix() {
            if (Main.player == null) return;
            StationController? NearestController = StationController.allStations.FindMin(cont => (PlayerManager.PlayerTransform.position - cont.transform.position).magnitude);
            NearestController?.RegenerateJobs();
        }
    }
}