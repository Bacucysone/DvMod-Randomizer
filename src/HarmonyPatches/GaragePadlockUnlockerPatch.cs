using DV.ThingTypes;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(GaragePadlockUnlocker))]
public class GaragePadlockUnlockerPatch {
    
    [HarmonyPrefix, HarmonyPatch(nameof(GaragePadlockUnlocker.OnGarageUnlocked))]
    public static void OnGarageUnlocked_Prefix(GarageType_v2 unlockedGarageType) {
        if (!Main.IsConnected) return;
        long id = RandoCommonData.GetIdFromGarage(unlockedGarageType);
        if (id > 0) Main.Player.UnlockCheck(id);
    }
}