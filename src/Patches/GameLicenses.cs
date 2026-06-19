
using System;
using System.Collections.Generic;
using DV;
using DV.RenderTextureSystem.BookletRender;
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
            if (!Main.IsConnected) return true;
            ProcessListOfIDs(data.GetStringArray("Licenses_General"), Globals.G.Types.generalLicenses).ForEach(__instance.AcquireGeneralLicense);

    		ProcessListOfIDs(data.GetStringArray("Licenses_Jobs"), Globals.G.Types.jobLicenses).ForEach(__instance.AcquireJobLicense);
		
    		ProcessListOfIDs(data.GetStringArray("Garages"), Globals.G.Types.garages).ForEach(__instance.UnlockGarage);

            return false;
        }
    }
    [HarmonyPatch(typeof(StaticLicenseBookletRender), nameof(StaticLicenseBookletRender.GetStaticTemplatePaperData))]
    public static class LocoHintPatcher {
        public static void Postfix(GeneralLicenseType_v2 ___generalLicense, ref TemplatePaperData[] __result) {
            if (!Main.IsConnected) return;
            int order = RandoCommonData.GetOrderFromLocoLicense(___generalLicense);
            if (order < 0 || !Main.Player.Config.HintsOnLocoLicense) return;
            LicenseTemplatePaperData firstPage = (LicenseTemplatePaperData) __result[0];
            firstPage.licenseDescription += $"\nIn {Main.Player.Config.LocoJobsThreshold[order]} job with this loco, you will earn a {Main.Player.GetItemNameFromLocationId(RandoCommonData.AP_ID.LOC_LOCO_NB_JOBS+order, true)}";
        }
    }
}