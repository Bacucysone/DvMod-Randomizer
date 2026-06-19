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

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(StartGameData_NewCareer))]
public class StartGameData_NewCareerPatch {

    [HarmonyPostfix, HarmonyPatch(nameof(StartGameData_NewCareer.DoLoad))]
    public static IEnumerator DoLoad_Postfix(IEnumerator originalMethod, Transform playerContainer) {
        yield return originalMethod;
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
    public static bool PrepareNewSaveData_Prefix(ref SaveGameData saveGameData, IGameSession session, IDifficulty difficultyParams) {
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