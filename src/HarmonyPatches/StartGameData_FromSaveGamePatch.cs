using System;
using System.Collections;
using System.Collections.Generic;
using DV.ThingTypes;
using DV.UI;
using DV.Utils;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace DvMod.Randomizer.HarmonyPatches;
/// <summary>
/// Prepare randomizer save data when starting a save file
/// </summary>
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

    [HarmonyPostfix, HarmonyPatch(nameof(StartGameData_FromSaveGame.LoadingNonBlockingCoro))]
    public static IEnumerator LoadingNonBlockingCoro_Postfix(IEnumerator originalMethod) {
        yield return originalMethod;
        foreach (GarageCarSpawner garageSpawner in GarageCarSpawner.Spawners.Values) {
            if (!Main.Player.HasUnlocked(garageSpawner.garageType)) continue;
            foreach (TrainCarLivery livery in garageSpawner.GarageCarLiveries) {
                if (garageSpawner.GetCar(livery) != null) continue;
                TrainCar foundCar = SingletonBehaviour<CarSpawner>.Instance.allCars.Find(car => car.carLivery == livery);
                SingletonBehaviour<CarSpawner>.Instance.DeleteTrainCars(SingletonBehaviour<CarSpawner>.Instance.allCars.FindAll(car => car.carLivery == livery && car != foundCar));
                garageSpawner.OverrideSpawnedCarReference(foundCar);
            }
        }
    }
}