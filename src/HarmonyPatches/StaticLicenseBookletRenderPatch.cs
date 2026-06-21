using DV.RenderTextureSystem.BookletRender;
using DV.ThingTypes;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(StaticLicenseBookletRender))]
public class StaticLicenseBookletRenderPatch {
    /// <summary>
    /// Add the hint of the item located on the sufficient number of jobs with a given locomotive on this locomotive license
    /// Only if it is a locomotive license and if the options allow it
    /// </summary>
    [HarmonyPostfix, HarmonyPatch(nameof(StaticLicenseBookletRender.GetStaticTemplatePaperData))]
    public static void GetStaticTemplatePaperData_Postfix(GeneralLicenseType_v2 ___generalLicense, ref TemplatePaperData[] __result) {
        if (!Main.IsConnected) return;
        int order = RandoCommonData.GetOrderFromLocoLicense(___generalLicense);
        if (order < 0 || !Main.Player.Config.HintsOnLocoLicense) return;
        LicenseTemplatePaperData firstPage = (LicenseTemplatePaperData) __result[0];
        firstPage.licenseDescription += $"\nIn {Main.Player.Config.LocoJobsThreshold[order]} job with this loco, you will earn a {Main.Player.GetItemNameFromLocationId(RandoCommonData.GetLocoNbJobsIdFromOrder(order), true)}";
    }
}