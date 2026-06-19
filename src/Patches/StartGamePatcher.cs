using System;
using System.Collections;
using DV.Common;
using DV.JObjectExtstensions;
using DV.Scenarios.Common;
using DV.UI;
using DV.UserManagement;
using DV.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace DvMod.Randomizer
{

    [HarmonyPatch(typeof(StartGameData_FromSaveGame))]
    public class LoadingPatch {
        private static void ExitWithMessage(string message) {
            Main.Error(message);
            MainMenu.GoBackToMainMenu();
            SceneManager.UnloadSceneAsync((int)DVScenes.Game);
            SingletonBehaviour<CoroutineManager>.Instance.StopCoroutine("LoadingRoutine");
        }
        [HarmonyPrefix, HarmonyPatch("Initialize")]
        public static void SaveLoadingPatch(bool ___initialized, out bool __state) => __state = ___initialized;
        
        [HarmonyPostfix, HarmonyPatch("Initialize")]
        public static void SaveLoadingEndPatch(SaveGameData ___saveGameData, bool __state) {
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
    [HarmonyPatch(typeof(SaveGameManager))]
    public class SavingPatch {
        [HarmonyPrefix, HarmonyPatch("UpdateInternalData")]
        public static void SavePrefix(SaveGameData ___data) {
            if (!Main.IsConnected) return;
            ___data.SetObject("RandoData", Main.Player.Data);
        }
    }

    [HarmonyPatch(typeof(StartGameData_NewCareer))]
    public class NewSavePatch {

        [HarmonyPostfix, HarmonyPatch(nameof(StartGameData_NewCareer.DoLoad))]
        public static IEnumerator TeleportPlayerPostfix(IEnumerator ret, Transform playerContainer) {
            yield return ret;
            Transform teleportAnchor = 
                StationController.allStations
                    .Find(sc => sc.stationInfo.YardID.Equals(Main.Player.SlotData.StartStation))
                    .stationRange
                    .stationCenterAnchor;
            playerContainer.position = teleportAnchor.position;
            playerContainer.rotation = teleportAnchor.rotation;
            yield return null;
        }
        [HarmonyPrefix, HarmonyPatch(nameof(StartGameData_NewCareer.PrepareNewSaveData))]
        public static bool Prefix(StartGameData_NewCareer __instance, ref SaveGameData saveGameData, IGameSession session, IDifficulty difficultyParams) {
            if (!Main.Settings!.CreateAPSave) return true;
            try {
                Main.Connect(null);
            } catch (TimeoutException) {
                Main.Log("Tried, but failed. Sorry");
                Main.Disconnect();
                MainMenu.GoBackToMainMenu();
                return false;
            }
            saveGameData ??= SaveGameManager.MakeEmptySave();
            saveGameData.Clear();
            saveGameData.SetString("Game_mode", session.GameMode);
            saveGameData.SetString("World", session.World);
            saveGameData.SetDouble("Starting_time_and_date", AStartGameData.BaseTimeAndDate.ToOADate());
            IDifficulty difficultyToUse = difficultyParams ?? DifficultyParamsSetter.Standard;
            DifficultyParamsSetter.SetDifficultyParams(difficultyToUse);
            session.PerformGameplayEntryDifficultyCheck(difficultyToUse);
            //SingletonBehaviour<CoroutineManager>.Instance.Run(TeleportPlayer());
            //__instance.DifficultyToUse = DifficultyToUse;
            saveGameData.SetFloat("Player_money", Main.Player.SlotData.Money);
            saveGameData.SetBool("Tutorial_01_completed", value: true);
            saveGameData.SetBool("Tutorial_02_completed", value: true);
            saveGameData.SetBool("Tutorial_03_completed", value: true);
            saveGameData.SetInt("Starting_items", 0);
            session.GameData.SetBool("Difficulty_picked", value: true);
            Main.Player.InitGame();
            return false;
        }
    }
    
}