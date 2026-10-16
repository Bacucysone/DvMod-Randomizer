using DV.ThingTypes;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(ResourceTypes))]
public class ResourceTypes_Patch {

    [HarmonyPostfix, HarmonyPatch(nameof(ResourceTypes.GetFullUnitPriceOfResource))]
    public static void GetFullUnitPriceOfResource_Postfix(ref float __result, ResourceType resource) {
        if (!Main.IsConnected) return;
        int order = RandoCommonData.GetOrderFromResourceType(resource);
        if (order >= 0 && Main.Player.HasUnlockedVoucher(order))
            __result /= 2f;
    }
}