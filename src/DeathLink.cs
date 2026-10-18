using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DvMod.Randomizer;
/// <summary>
/// Allows for deathlink (when activated, when a death occurs somewhere in AP multiworld, a deathlink is sent to all registered players)
/// In case of Derail Valley, "death" is derailing
/// </summary>
[HarmonyPatch(typeof(TrainCar))]
public class DeathLinkPatch {
    private static float LastDeathlink = 0f; 

    public static void Derail(DeathLink _) {
        LastDeathlink = Time.time;
        (PlayerManager.Car ?? PlayerManager.LastLoco)?.DerailAllBogies("due to deathlink");
    }
    
    [HarmonyPrefix, HarmonyPatch(nameof(TrainCar.Derail))]
    public static void Derail_Prefix(TrainCar __instance) {
        if (__instance.derailed || !Main.IsConnected) return;
        if (__instance != PlayerManager.Car && __instance != PlayerManager.LastLoco) return;
        if (Time.time - LastDeathlink <= 15f) return;
        LastDeathlink = Time.time;
        string playerName = Main.Player.Session.Players.ActivePlayer.Name;
        Main.Player.deathLinkService.SendDeathLink(new DeathLink(playerName, playerName + " derailed"));
        
    }
}