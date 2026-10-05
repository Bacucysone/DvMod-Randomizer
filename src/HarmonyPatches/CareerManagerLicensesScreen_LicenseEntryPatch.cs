using System.Collections.Generic;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using DV.Localization;
using DV.ServicePenalty.UI;
using DV.Utils;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;
/// <summary>
/// Change the career manager behaviour when buying licenses: Change license selection screen to have different
/// acquired and obtainable criteria
/// </summary>
[HarmonyPatch(typeof(CareerManagerLicensesScreen.LicenseEntry))]
public class CareerManagerLicensesScreen_LicenseEntryPatch {

    [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensesScreen.LicenseEntry.UpdateJobLicenseData))]
    public static void UpdateJobLicenseData_Postfix(CareerManagerLicensesScreen.LicenseEntry __instance) {
        if (!Main.IsConnected) return;
        __instance.IsAcquired = Main.Player.HasChecked(__instance.JobLicense);
        __instance.IsObtainable = !__instance.IsAcquired && 
            (__instance.JobLicense.requiredGeneralLicense == null ||
             SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(__instance.JobLicense
                 .requiredGeneralLicense)) &&
            (__instance.JobLicense.requiredJobLicense == null ||
             SingletonBehaviour<LicenseManager>.Instance.IsJobLicenseAcquired(__instance.JobLicense
                 .requiredJobLicense));
        long locationId = RandoCommonData.GetIdFromJobLicense(__instance.JobLicense);
        if (locationId >= 0 && Main.Player.Config.HintsLicenseManager) {
            Task<Dictionary<long, ScoutedItemInfo>> hintTask =
                Main.Player.Session.Locations.ScoutLocationsAsync(HintCreationPolicy.CreateAndAnnounceOnce, locationId);
            hintTask.Wait();
            ScoutedItemInfo hintInfo = hintTask.Result[locationId];
            __instance.name.text = hintInfo.ItemDisplayName + " (" + hintInfo.Player.Alias + ")";
        } else
            __instance.name.text = "AP Item";
        if (!__instance.IsAcquired){
            __instance.status.text = "$" + __instance.JobLicense.price.ToString("N2", LocalizationAPI.CC);
        } else 
            __instance.status.text = CareerManagerLocalization.OWNED;
        
    }
    
    [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensesScreen.LicenseEntry.UpdateGeneralLicenseData))]
    public static void UpdateGeneralLicenseData_Postfix(CareerManagerLicensesScreen.LicenseEntry __instance) {
        if (!Main.IsConnected) return;
        __instance.IsAcquired = Main.Player.HasChecked(__instance.GeneralLicense);
        __instance.IsObtainable = !__instance.IsAcquired && 
                (__instance.GeneralLicense.requiredGeneralLicense == null ||
                    SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(__instance.GeneralLicense
                 .requiredGeneralLicense)) &&
                (__instance.GeneralLicense.requiredJobLicense == null ||
                    SingletonBehaviour<LicenseManager>.Instance.IsJobLicenseAcquired(__instance.GeneralLicense
                 .requiredJobLicense));
        long locationId = RandoCommonData.GetIdFromGeneralLicense(__instance.GeneralLicense);
        if (locationId >= 0 && Main.Player.Config.HintsLicenseManager) {
            Task<Dictionary<long, ScoutedItemInfo>> hintTask =
                Main.Player.Session.Locations.ScoutLocationsAsync(HintCreationPolicy.CreateAndAnnounceOnce, locationId);
            hintTask.Wait();
            ScoutedItemInfo hintInfo = hintTask.Result[locationId];
            __instance.name.text = hintInfo.ItemDisplayName + " (" + hintInfo.Player.Alias + ")";
        } else
            __instance.name.text = "AP Item";
        if (!__instance.IsAcquired){
            __instance.status.text = "$" + __instance.GeneralLicense.price.ToString("N2", LocalizationAPI.CC);
        } else 
            __instance.status.text = CareerManagerLocalization.OWNED;
    }
}