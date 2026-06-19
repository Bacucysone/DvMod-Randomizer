using DV.ThingTypes;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(GaragePadlockUnlocker))]
public class GaragePadlockUnlockerPatch {
    
    [HarmonyPrefix, HarmonyPatch(nameof(GaragePadlockUnlocker.OnGarageUnlocked))]
    public static void OnGarageUnlocked_Prefix(GarageType_v2 unlockedGarageType) {
        if (!Main.IsConnected) return;
        switch (unlockedGarageType.v1) {
            case Garage.Caboose: Main.Player.UnlockCheck(0x691); break;
            case Garage.DM1U: Main.Player.UnlockCheck(0x693); break;
            case Garage.Bob: Main.Player.UnlockCheck(0x692); break;
            case Garage.DE6_Slug: Main.Player.UnlockCheck(0x690); break;
        }
    }
}