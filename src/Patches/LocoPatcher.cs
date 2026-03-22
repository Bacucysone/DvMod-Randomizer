using HarmonyLib;
using DV;
using System.Collections.Generic;
using DV.ThingTypes;
using DV.Utils;
using CommandTerminal;
using System.Linq;
using DV.ThingTypes.TransitionHelpers;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(CommsRadioCrewVehicle))]
    public static class CrewCommsPatch {
        [HarmonyPostfix, HarmonyPatch("UpdateAvailableVehicles")]
        public static void CustomVehicles(ref List<TrainCarLivery> ___availableVehiclesForSpawn) {
            if (Main.player == null) 
                return;
            Garage[] crewVehicleGarages = [
                Garage.Bob,
                Garage.Caboose,
                Garage.DM1U,
                Garage.DE6_Slug,
                Garage.Museum_FlatbedShort,
                Garage.DE2_Relic,
                Garage.DM3_Relic,
                Garage.DH4_Relic,
                Garage.DE6_Relic,
                Garage.S060_Relic,
                Garage.S282_Relic
            ];
            ___availableVehiclesForSpawn = 
                    crewVehicleGarages
                    .Select(g => g.ToV2())
                    .Where(Main.player!.HasUnlocked)
                    .SelectMany(g => g.garageCarLiveries)
                    .ToList()??[];
        }
    }

    [HarmonyPatch(typeof(GaragePadlockUnlocker), "OnGarageUnlocked")]
    public static class GaragePatcher {
        public static void Prefix(GarageType_v2 unlockedGarageType) {
            if (Main.player == null) return;
            switch (unlockedGarageType.v1) {
                case Garage.Caboose: Main.player.UnlockCheck(0x691); break;
                case Garage.DM1U: Main.player.UnlockCheck(0x693); break;
                case Garage.Bob: Main.player.UnlockCheck(0x692); break;
                case Garage.DE6_Slug: Main.player.UnlockCheck(0x690); break;
            }
        }
    }
}