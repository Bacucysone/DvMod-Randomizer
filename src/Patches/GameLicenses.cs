
using System;
using System.Collections.Generic;
using DV;
using DV.RenderTextureSystem.BookletRender;
using DV.ThingTypes;
using DV.UI;
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
    [HarmonyPatch(typeof(StaticLicenseBookletRender), nameof(StaticLicenseBookletRender.GetStaticTemplatePaperData))]
    public static class LocoHintPatcher {
        public static void Postfix(GeneralLicenseType_v2 ___generalLicense, ref TemplatePaperData[] __result) {
            if (Main.player == null) return;
            int Order = RandoCommonData.GetOrderFromLocoLicense(___generalLicense);
            if (Order < 0 || !Main.player.Data.Config.HintsOnLocoLicense) return;
            LicenseTemplatePaperData FirstPage = (LicenseTemplatePaperData) __result[0];
            FirstPage.licenseDescription += $"\nIn {Main.player.Data.Config.LocoJobsThreshold[Order]} job with this loco, you will earn a {Main.player.GetItemNameFromLocationId(RandoCommonData.AP_ID.LOC_LOCO_NB_JOBS+Order, true)}";
        }
    }
}