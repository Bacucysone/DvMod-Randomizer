using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using Archipelago.MultiClient.Net;
using DV.Teleporters;
using DV.UI;
using HarmonyLib;
using I2.Loc;
using UnityEngine;


namespace DvMod.Randomizer
{

    [HarmonyPatch(typeof(StartGameData_FromSaveGame))]
    public class LoadingPatch {

        [HarmonyPostfix, HarmonyPatch("Initialize")]
        public static void SaveLoadingEndPatch(SaveGameData ___saveGameData) {
            RandoSaveData? data = ___saveGameData.GetObject<RandoSaveData>("RandoData");
            if (data == null) {
                Main.Log("Launching game in normal mode");
                return;
            }
            if (data.Version != 1) {
                Main.Error($"Randomizer detected but versions do not match: Mod version = 1/Save version = {data.Version}");
                MainMenu.GoBackToMainMenu();
                return;
            }
            Main.player ??= new(data);
        }
        [HarmonyPatch(nameof(StartGameData_FromSaveGame.ShouldCreateSaveGameAfterLoad))]
        public static void Postfix(ref bool __result) {
            if (Main.player == null) return;
            __result = Main.player.IsFirstLoading;
        }

        [HarmonyPatch(nameof(StartGameData_FromSaveGame.GetPostLoadMessage))]
        public static void Postfix(ref string __result) {
            if (Main.player == null) return;
            if (__result != null && Main.player.IsFirstLoading) 
                __result = "Rando/Start";
                
        }
    }
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
    public class TranslationPatch {
        public static void Postfix(string Term, ref string __result) {
            switch (Term) {
                case "Rando/Start":
                __result = "Welcome to randomizer Derail Valley! Gather the different station licenses and complete enough jobs to finish the game. Have fun!";
                break;
            }
        }
    }
        
    [HarmonyPatch(typeof(SaveGameManager))]
    public class SavingPatch {
        [HarmonyPrefix, HarmonyPatch("UpdateInternalData")]
        public static void SavePrefix(SaveGameData ___data) {
            if (Main.player == null) return;
            ___data.SetObject("RandoData", Main.player.data);
        }
    }
    
}