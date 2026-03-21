using DV.Booklets;
using DV.ServicePenalty.UI;
using DV.Shops;
using DV.ThingTypes;
using HarmonyLib;
using TMPro;
using DV.Localization;
using Archipelago.MultiClient.Net.Models;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(CareerManagerLicensesScreen.LicenseEntry))]
    public static class CareerManagerLicensesPatcher {
        [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensesScreen.LicenseEntry.UpdateJobLicenseData))]
        public static void JobLicensesInfoPatch(CareerManagerLicensesScreen.LicenseEntry __instance) {
            if (Main.player == null) return;
            __instance.IsAcquired = Main.player.HasChecked(__instance.JobLicense);
            __instance.IsObtainable |= __instance.JobLicense.v1 == JobLicenses.FreightHaul;
            if (!__instance.IsAcquired){
                __instance.status.text = "$" + __instance.JobLicense.price.ToString("N2", LocalizationAPI.CC);
                __instance.name.text += "?";
            } else 
                __instance.status.text = CareerManagerLocalization.OWNED;
            
        }
        [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensesScreen.LicenseEntry.UpdateGeneralLicenseData))]
        public static void GeneralLicensesInfoPatch(CareerManagerLicensesScreen.LicenseEntry __instance) {
            if (Main.player == null) return;
            __instance.IsAcquired = Main.player.HasChecked(__instance.GeneralLicense);
            __instance.IsObtainable |= __instance.GeneralLicense.v1 == GeneralLicenseType.TrainDriver || __instance.GeneralLicense.v1 == GeneralLicenseType.DE2;
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
            if (Main.player == null) return true;
            if (input != InputAction.Confirm) return true;
            if (!__instance.cashReg.Buy()) return true;
            float price;
            ItemInfo item;
            if (___generalLicenseToBuy != null) {
                item = Main.player.UnlockCheck(RandoCommonData.GetIDFromGeneralLicense(___generalLicenseToBuy).Item1);
                price = ___generalLicenseToBuy.price;
            } else {
                item = Main.player.UnlockCheck(RandoCommonData.GetIDFromJobLicense(___jobLicenseToBuy).Item1);
                price = ___jobLicenseToBuy.price;
            }
            CashRegisterModule ToPrint = new GenericThingCashRegisterModule();
            string itemName = item.ItemDisplayName;
            ToPrint.Data.unitsToBuy = 1;
            ToPrint.Data.pricePerUnit = price;
            ToPrint.Data.resourceName = itemName;
            BookletCreator.CreateCashRegisterReceipt([ToPrint], __instance.licensePrinter.spawnAnchor.position, __instance.licensePrinter.spawnAnchor.rotation, WorldMover.OriginShiftParent);
            __instance.licensePrinter.Print();
            __instance.screenSwitcher.SetActiveDisplay(__instance.licensesScreen);
            return false; 
        }
    }
}