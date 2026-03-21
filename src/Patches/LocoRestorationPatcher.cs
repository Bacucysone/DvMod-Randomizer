using System;
using DV;
using DV.LocoRestoration;
using DV.OriginShift;
using DV.ThingTypes;
using DV.Utils;
using HarmonyLib;
using UnityEngine;

namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(PaintStationItemInstantiator), "Awake")]
    public class PainterItemInstantiatorPatch {
        public static bool Prefix() => Main.player == null;
    }
    [HarmonyPatch(typeof(LocoRestorationController))]
    public static class LocoRestorationPatcher {

        [HarmonyPrefix, HarmonyPatch("InitCarForRestoration")]
        public static bool Prefix(LocoRestorationController __instance, TrainCar car){
            if (Main.player == null) return true;
            if (__instance.State == LocoRestorationController.RestorationState.S0_Initialized 
             || __instance.State == LocoRestorationController.RestorationState.S1_UnlockedRestorationLicense 
             || __instance.State == LocoRestorationController.RestorationState.S2_LocoUnblocked
             || __instance.State == LocoRestorationController.RestorationState.S3_RerailedCars) {
                SingletonBehaviour<CarSpawner>.Instance.DeleteCar(car);
                return false;
            }
            return true;
        }
        [HarmonyPostfix, HarmonyPatch("DeliverPartCoro")]
        public static void PartsPostfix(TrainCar ___loco, LocoRestorationController __instance) {
            if (Main.player == null) return;
            Main.player.UnlockCheck(RandoCommonData.AP_ID.LOC_RELIC_PARTS+RandoCommonData.GetOrderFromLocoType(___loco.carType));
            if (!Main.player.CanFinishRelic(___loco.carType)) {
                __instance.installPartsModule.ThingBought -= __instance.OnInstallPartsPaid;
                __instance.installPartsModule.SetUnitsToBuy(0f);
            }

        }

        [HarmonyPostfix, HarmonyPatch("SetupListenersForPaintJob")]
        public static void PaintPostfix(TrainCar ___loco, bool on) {
            if (Main.player != null && !on) Main.player.UnlockCheck(RandoCommonData.AP_ID.LOC_RELIC_PAINTED+RandoCommonData.GetOrderFromLocoType(___loco.carType));
        }
    }
}