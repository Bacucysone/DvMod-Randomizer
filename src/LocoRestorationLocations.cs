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
            if (Input.GetKeyDown("[0]")) {
                Input.ResetInputAxes();
                foreach(GeneralLicenseType_v2 license in Globals.G.Types.generalLicenses) {
                    Main.Log($"License {license.v1.ToString()}, price {license.price}, GRequired {license.requiredGeneralLicense}, JRequired {license.requiredJobLicense}");
                }
                foreach(JobLicenseType_v2 license in Globals.G.Types.jobLicenses) {
                    Main.Log($"License {license.v1.ToString()}, price {license.price}, GRequired {license.requiredGeneralLicense}, JRequired {license.requiredJobLicense}");
                }
            }
            Main.player?.CallUpdate();
        }
    }
}