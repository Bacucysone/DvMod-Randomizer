using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(PaintStationItemInstantiator))]
public class PaintStationItemInstantiatorPatch {
    /// <summary>
    /// Remove the spawn of the paint sprayer in the museum to lock the progression behind an AP item
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(nameof(PaintStationItemInstantiator.Awake))]
    public static bool Prefix() => !Main.IsConnected;
}