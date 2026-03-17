using System.Collections.Generic;
using CommandTerminal;
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
            }
            if (Main.player == null) return;
            if (!WorldStreamingInit.IsLoaded) return;
            Main.player.CallUpdate();
            (Vector3 closestPoint, float Distance) = RandoCommonData.GetClosestLocoLocation(PlayerManager.PlayerTransform.AbsolutePosition());
            if (Distance < 5f) {
                if (!Main.player.HasChecked(closestPoint)) {
                    string stationNeeded = RandoCommonData.GetStationFromLocoLocations(closestPoint);
                    bool StationOk = Main.player.GotStationLicense(stationNeeded);
                    bool MuseumOk = SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2());
                    if (StationOk && MuseumOk) {
                        Terminal.Log(TerminalLogType.Input, "You found something on the ground!");
                        Main.player.NotifyPlayer("You found something on the ground!");
                        Main.player.UnlockCheck(0x400+RandoCommonData.GetIdFromLocoLocations(closestPoint));
                    } else if (StationOk && !MuseumOk)
                        Main.player.NotifyPlayer("There is something here but you cannot take it... You need the museum license");
                    else if (!StationOk && MuseumOk)
                        Main.player.NotifyPlayer("There is something here but you cannot take it... You need the "+stationNeeded+" station license");
                    else
                        Main.player.NotifyPlayer("There is something here but you cannot take it... You need the museum license and the "+stationNeeded+" station license");
                }
            }
        }
    }
}