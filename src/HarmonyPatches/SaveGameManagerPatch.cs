using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(SaveGameManager))]
public class SaveGameManagerPatch {
    
    [HarmonyPrefix, HarmonyPatch("UpdateInternalData")]
    public static void UpdateInternalData_Prefix(SaveGameData ___data) {
        if (!Main.IsConnected) return;
        ___data.SetObject("RandoData", Main.Player.Data);
    }
}