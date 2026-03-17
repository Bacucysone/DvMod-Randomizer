using DV.Booklets;
using DV.ServicePenalty.UI;
using DV.Shops;
using DV.ThingTypes;
using HarmonyLib;
using TMPro;
using DV.Localization;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(CareerManagerLicensesScreen.LicenseEntry))]
    public static class CareerManagerLicensesPatcher {
        [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensesScreen.LicenseEntry.UpdateJobLicenseData))]
        public static void JobLicensesInfoPatch(CareerManagerLicensesScreen.LicenseEntry __instance) {
            if (Main.player == null) return;
            __instance.IsAcquired = Main.player.HasChecked(__instance.JobLicense);
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
            long id;
            if (___generalLicenseToBuy != null) {
                Main.player.Check(___generalLicenseToBuy);
                price = ___generalLicenseToBuy.price;
                id = RandoCommonData.GetIDFromGeneralLicense(___generalLicenseToBuy);
            } else {
                Main.player.Check(___jobLicenseToBuy);
                price = ___jobLicenseToBuy.price;
                id = RandoCommonData.GetIDFromJobLicense(___jobLicenseToBuy);
            }
            CashRegisterModule ToPrint = new GenericThingCashRegisterModule();
            string itemName = Main.player.GetItemNameFromLocationId(id);
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