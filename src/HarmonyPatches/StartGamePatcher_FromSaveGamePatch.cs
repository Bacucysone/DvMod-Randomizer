using System;
using DV.UI;
using DV.Utils;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(StartGameData_FromSaveGame))]
public class StartGameData_FromSaveGamePatch {
    private static void ExitWithMessage(string message) {
        Main.Error(message);
        MainMenu.GoBackToMainMenu();
        SceneManager.UnloadSceneAsync((int)DVScenes.Game);
        SingletonBehaviour<CoroutineManager>.Instance.StopCoroutine("LoadingRoutine");
    }
    
    [HarmonyPrefix, HarmonyPatch("Initialize")]
    public static void Initialize_Prefix(bool ___initialized, out bool __state) => __state = ___initialized;
        
    [HarmonyPostfix, HarmonyPatch("Initialize")]
    public static void Initialize_Postfix(SaveGameData ___saveGameData, bool __state) {
        if (__state) return;
        RandoSaveData data = ___saveGameData.GetObject<RandoSaveData>("RandoData");
        if (data == null) {
            Main.Log("Launching game in normal mode");
            return;
        }
        if (data.Version != Main.VERSION) {
            ExitWithMessage($"Randomizer detected but versions do not match: Mod version = {Main.VERSION}/Save version = {data.Version}. Returning to main menu...");
            return;
        }
        try {
            Main.Connect(data);
        } catch (TimeoutException) {
            ExitWithMessage($"Could not connect to server. Returning to main menu...");
            Main.Disconnect();
            return;
        }
        Main.Player.InitGame();
    }
}