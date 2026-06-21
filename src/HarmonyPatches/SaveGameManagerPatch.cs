using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(SaveGameManager))]
public class SaveGameManagerPatch {
    /// <summary>
    /// When saving to file, adding the randomizer save data as well
    /// </summary>
    [HarmonyPrefix, HarmonyPatch("UpdateInternalData")]
    public static void UpdateInternalData_Prefix(SaveGameData ___data) {
        if (!Main.IsConnected) return;
        ___data.SetObject("RandoData", Main.Player.Data);
    }
}