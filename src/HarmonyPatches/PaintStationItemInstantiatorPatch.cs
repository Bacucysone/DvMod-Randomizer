using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(PaintStationItemInstantiator))]
public class PaintStationItemInstantiatorPatch {
    
    [HarmonyPrefix, HarmonyPatch(nameof(PaintStationItemInstantiator.Awake))]
    public static bool Prefix() => !Main.IsConnected;
}