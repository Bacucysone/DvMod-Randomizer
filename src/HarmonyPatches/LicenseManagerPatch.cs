using System;
using System.Collections.Generic;
using DV;
using DV.ThingTypes;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(LicenseManager))]
public class LicenseManagerPatch {
    public static List<T> ProcessListOfIDs<T>(string[] ids, List<T> refs) where T: Thing_v2 {
        List<T> ret = [];
        if (ids == null) return ret;
        Array.ForEach(ids, s => ret.Add(refs.Find(x => x.id == s)));
        
        return ret;
    }
    
    [HarmonyPrefix, HarmonyPatch(nameof(LicenseManager.LoadData))]
    public static bool LoadData_Prefix(SaveGameData data, LicenseManager __instance) {
        if (!Main.IsConnected) return true;
        ProcessListOfIDs(data.GetStringArray("Licenses_General"), Globals.G.Types.generalLicenses).ForEach(__instance.AcquireGeneralLicense);

        ProcessListOfIDs(data.GetStringArray("Licenses_Jobs"), Globals.G.Types.jobLicenses).ForEach(__instance.AcquireJobLicense);
		
        ProcessListOfIDs(data.GetStringArray("Garages"), Globals.G.Types.garages).ForEach(__instance.UnlockGarage);

        return false;
    }
}
