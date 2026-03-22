
using System;
using System.Collections.Generic;
using DV;
using DV.ThingTypes;
using HarmonyLib;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(LicenseManager), nameof(LicenseManager.LoadData))]
    public static class LicensePatch {
        public static List<T> ProcessListOfIDs<T>(string[] ids, List<T> refs) where T: Thing_v2 {
            List<T> ret = [];
            if (ids == null) return ret;
            Array.ForEach(ids, s => ret.Add(refs.Find(x => x.id == s)));
        
            return ret;
        }
        public static bool Prefix(SaveGameData data, LicenseManager __instance) {
            if (Main.player == null) return true;
            ProcessListOfIDs(data.GetStringArray("Licenses_General"), Globals.G.Types.generalLicenses).ForEach(__instance.AcquireGeneralLicense);

    		ProcessListOfIDs(data.GetStringArray("Licenses_Jobs"), Globals.G.Types.jobLicenses).ForEach(__instance.AcquireJobLicense);
		
    		ProcessListOfIDs(data.GetStringArray("Garages"), Globals.G.Types.garages).ForEach(__instance.UnlockGarage);

            return false;
        }
    }
}