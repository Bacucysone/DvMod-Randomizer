using System.Collections;
using DV.LocoRestoration;
using DV.Utils;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(LocoRestorationController))]
public static class LocoRestorationControllerPatch {
        
    [HarmonyPostfix, HarmonyPatch("Start")]
    public static IEnumerator Start_Postfix(IEnumerator originalMethod, LocoRestorationController __instance) {
        yield return originalMethod; 
        if (!Main.IsConnected) yield break;
        if (__instance.State >= LocoRestorationController.RestorationState.S4_OnDestinationTrack) yield break;
        __instance.loco.OnDestroyCar -= __instance.OnUnexpectedDestroy;
        SingletonBehaviour<CarSpawner>.Instance.DeleteCar(__instance.loco);
        if (__instance.secondCar == null) yield break;
        __instance.secondCar.OnDestroyCar -= __instance.OnUnexpectedDestroy;
        SingletonBehaviour<CarSpawner>.Instance.DeleteCar(__instance.secondCar);
    }
    
    [HarmonyPostfix, HarmonyPatch("DeliverPartCoro")]
    public static IEnumerator DeliverPartCoro_Postfix(IEnumerator originalMethod, TrainCar ___loco, LocoRestorationController __instance) {
        yield return originalMethod;
        if (!Main.IsConnected) yield break;
        Main.Player.UnlockCheck(RandoCommonData.AP_ID.LOC_RELIC_PARTS+RandoCommonData.GetOrderFromLocoType(___loco.carType));
        if (Main.Player.CanFinishRelic(___loco.carType)) yield break;
        __instance.installPartsModule.ThingBought -= __instance.OnInstallPartsPaid;
        __instance.installPartsModule.SetUnitsToBuy(0f);
    }

    [HarmonyPostfix, HarmonyPatch("SetupListenersForPaintJob")]
    public static void SetupListenersForPaintJob_Postfix(TrainCar ___loco, bool on) {
        if (Main.IsConnected && !on) Main.Player.UnlockCheck(RandoCommonData.AP_ID.LOC_RELIC_PAINTED+RandoCommonData.GetOrderFromLocoType(___loco.carType));
    }
}