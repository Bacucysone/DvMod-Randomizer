using UnityEngine;
using CommandTerminal;
using HarmonyLib;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(Terminal))]
    public static class RandoConsole {
        [HarmonyPrefix, HarmonyPatch("EnterCommand")]
        public static bool TerminalPatch(ref string ___command_text) {
            if (Main.player == null) return true;
            Main.player.Session.Say(___command_text);
            Terminal.Log(TerminalLogType.Input, Main.player.Session.Players.ActivePlayer.Name+":"+___command_text);
            ___command_text = "";
            return false;

        }
    }
}