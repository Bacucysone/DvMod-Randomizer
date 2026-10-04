using System;
using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using System.Threading.Tasks;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(SaveGameManager))]
public class SaveGameManagerPatch {
    /// <summary>
    /// When saving to file, adding the randomizer save data as well
    /// </summary>
    [HarmonyPrefix, HarmonyPatch("UpdateInternalData")]
    public static void UpdateInternalData_Prefix(SaveGameData ___data) {
        if (!Main.IsConnected) return;
        string guid = Guid.NewGuid().ToString("N");
        Main.Player.Data.Guid = guid;
        Task.Run(() => Main.Player.Session.DataStorage[Scope.Slot, "guid"] = guid);
        ___data.SetObject("RandoData", Main.Player.Data);
    }
}