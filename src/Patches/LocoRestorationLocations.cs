using System.Collections.Generic;
using CommandTerminal;
using DV;
using DV.Customization.Paint;
using DV.Damage;
using DV.LocoRestoration;
using DV.OriginShift;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using HarmonyLib;
using LocoSim.Implementations;
using UnityEngine;

namespace DvMod.Randomizer {
    [HarmonyPatch(typeof(SimulationFlow), "Tick")]
    public static class UpdatePatch {
        public static void Postfix() {
            Main.player?.CallUpdate();
        }
    }
}