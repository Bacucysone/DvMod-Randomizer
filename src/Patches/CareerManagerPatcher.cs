using DV.Booklets;
using DV.ServicePenalty.UI;
using DV.Shops;
using DV.ThingTypes;
using HarmonyLib;
using TMPro;
using DV.Localization;
using Archipelago.MultiClient.Net.Models;
using DV.Utils;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(CareerManagerLicensesScreen.LicenseEntry))]
    public static class CareerManagerLicensesPatcher {
        [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensesScreen.LicenseEntry.UpdateJobLicenseData))]
        public static void JobLicensesInfoPatch(CareerManagerLicensesScreen.LicenseEntry __instance) {
            if (!Main.IsConnected) return;
            __instance.IsAcquired = Main.Player.HasChecked(__instance.JobLicense);
            __instance.IsObtainable = !__instance.IsAcquired && 
                (__instance.JobLicense.requiredGeneralLicense == null ||
                 SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(__instance.JobLicense
                     .requiredGeneralLicense)) &&
                (__instance.JobLicense.requiredJobLicense == null ||
                 SingletonBehaviour<LicenseManager>.Instance.IsJobLicenseAcquired(__instance.JobLicense
                     .requiredJobLicense));
            if (!__instance.IsAcquired){
                __instance.status.text = "$" + __instance.JobLicense.price.ToString("N2", LocalizationAPI.CC);
                __instance.name.text += "?";
            } else 
                __instance.status.text = CareerManagerLocalization.OWNED;
            
        }
        [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensesScreen.LicenseEntry.UpdateGeneralLicenseData))]
        public static void GeneralLicensesInfoPatch(CareerManagerLicensesScreen.LicenseEntry __instance) {
            if (!Main.IsConnected) return;
            __instance.IsAcquired = Main.Player.HasChecked(__instance.GeneralLicense);
            __instance.IsObtainable = !__instance.IsAcquired && 
                    (__instance.GeneralLicense.requiredGeneralLicense == null ||
                        SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(__instance.GeneralLicense
                     .requiredGeneralLicense)) &&
                    (__instance.GeneralLicense.requiredJobLicense == null ||
                        SingletonBehaviour<LicenseManager>.Instance.IsJobLicenseAcquired(__instance.GeneralLicense
                     .requiredJobLicense));
            if (!__instance.IsAcquired){
                __instance.status.text = "$" + __instance.GeneralLicense.price.ToString("N2", LocalizationAPI.CC);
                __instance.name.text += "?";
            } else 
                __instance.status.text = CareerManagerLocalization.OWNED;
        }
    }

    [HarmonyPatch(typeof(CareerManagerLicensePayingScreen))]
    public static class CareerManagerLicensePayPatcher {
        [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensePayingScreen.Activate))]
        public static void NamePatcher(TextMeshPro ___licenseNameText) => ___licenseNameText.text += "?";
        [HarmonyPrefix, HarmonyPatch(nameof(CareerManagerLicensePayingScreen.HandleInputAction))]
        public static bool BuyingPatch(InputAction input, CareerManagerLicensePayingScreen __instance, JobLicenseType_v2 ___jobLicenseToBuy, GeneralLicenseType_v2 ___generalLicenseToBuy) {
            if (!Main.IsConnected) return true;
            if (input != InputAction.Confirm) return true;
            if (!__instance.cashReg.Buy()) return true;
            float price;
            ItemInfo item;
            if (___generalLicenseToBuy != null) {
                (long id, int _) = RandoCommonData.GetIDFromGeneralLicense(___generalLicenseToBuy);
                item = Main.Player.UnlockCheck(id);
                Main.Player.CheckGLicense(id);
                price = ___generalLicenseToBuy.price;
            } else {
                (long id, int _) = RandoCommonData.GetIDFromJobLicense(___jobLicenseToBuy);
                item = Main.Player.UnlockCheck(id);
                Main.Player.CheckJLicense(id);
                price = ___jobLicenseToBuy.price;
            }
            CashRegisterModule toPrint = new GenericThingCashRegisterModule();
            string itemName = item.ItemDisplayName+" ("+item.Player.Name+")";
            toPrint.Data.unitsToBuy = 1;
            toPrint.Data.pricePerUnit = price;
            toPrint.Data.resourceName = itemName;
            BookletCreator.CreateCashRegisterReceipt([toPrint], __instance.licensePrinter.spawnAnchor.position, __instance.licensePrinter.spawnAnchor.rotation, WorldMover.OriginShiftParent);
            __instance.licensePrinter.Print();
            __instance.screenSwitcher.SetActiveDisplay(__instance.licensesScreen);
            return false; 
        }
    }
}