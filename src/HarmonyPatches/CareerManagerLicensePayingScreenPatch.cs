using Archipelago.MultiClient.Net.Models;
using DV.Booklets;
using DV.ServicePenalty.UI;
using DV.Shops;
using DV.ThingTypes;
using HarmonyLib;
using TMPro;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(CareerManagerLicensePayingScreen))]
public class CareerManagerLicensePayingScreenPatch {
    
    [HarmonyPostfix, HarmonyPatch(nameof(CareerManagerLicensePayingScreen.Activate))]
    public static void Activate_Postfix(TextMeshPro ___licenseNameText) => ___licenseNameText.text += "?";
    
    /// <summary>
    /// Change the career manager behaviour when buying a new license: stop the license acquisition and send an AP item instead
    /// </summary>
    [HarmonyPrefix, HarmonyPatch(nameof(CareerManagerLicensePayingScreen.HandleInputAction))]
    public static bool HandleInputAction_Prefix(InputAction input, CareerManagerLicensePayingScreen __instance, JobLicenseType_v2 ___jobLicenseToBuy, GeneralLicenseType_v2 ___generalLicenseToBuy) {
        if (!Main.IsConnected) return true;
        if (input != InputAction.Confirm) return true;
        if (!__instance.cashReg.Buy()) return true;
        float price;
        ItemInfo item;
        if (___generalLicenseToBuy != null) {
            long id = RandoCommonData.GetIdFromGeneralLicense(___generalLicenseToBuy);
            item = Main.Player.UnlockCheck(id);
            Main.Player.CheckGLicense(___generalLicenseToBuy);
            price = ___generalLicenseToBuy.price;
        } else {
            long id = RandoCommonData.GetIdFromJobLicense(___jobLicenseToBuy);
            item = Main.Player.UnlockCheck(id);
            Main.Player.CheckJLicense(___jobLicenseToBuy);
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