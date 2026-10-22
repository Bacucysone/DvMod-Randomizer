using System.Linq;
using DV.ServicePenalty.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace DvMod.Randomizer.HarmonyPatches;

public class CareerManagerRandomizerInfos : DisplayScreen {
    
    private CareerManagerMainScreen _mainScreen;
    
    public void Awake() =>
        _mainScreen = transform.parent.GetComponentInChildren<CareerManagerMainScreen>();
    
    public override void Activate(IDisplayScreen _) {
        (TextMeshPro text, TextMeshPro value)[] lines = new (TextMeshPro text, TextMeshPro value) [14];
        foreach (TextMeshPro component in _mainScreen.fees.transform.parent.gameObject
                     .GetComponentsInChildren<TextMeshPro>()) {
            if (component.name.StartsWith("line")) {
                lines[int.Parse(component.name.Substring(4)) - 1].text = component;
            }
            else if (component.name.StartsWith("value")) {
                lines[int.Parse(component.name.Substring(5)) - 1].value = component;
            }
        }

        if (_mainScreen.gameObject.GetComponentInParent<TrainCarInteriorObject>() != null) {
            lines[0].text.text = "Randomizer info";
            lines[0].value.text = "Caboose";
            lines[1].text.text = "Double tokens";
            lines[1].value.text = Main.Player.Data.Tokens.ToString();
            int stationsFinished = Main.Player.Data.Freights
                .Zip(Main.Player.Data.Shunts, (x, y) => x + y)
                .Count(sum => sum >= Main.Player.Config.VictoryThreshold);
            
            lines[3].text.text = "Total victory: "; 
            lines[3].value.text = stationsFinished >= Main.Player.Config.Victory ? 
                                "Done" : 
                                stationsFinished + " / " + Main.Player.Config.Victory;
            
        }
        else {
            string station = StationController.allStations.FindMin(controller =>
                (controller.transform.position - Main.Player.Position).magnitude).stationInfo.YardID;
            int order = RandoCommonData.GetOrderFromStationName(station);
            lines[0].text.text = "Randomizer info";
            Main.Log($"For title: {lines[0].text} has been changed to {lines[0].text.text}");
            lines[0].value.text = station;
            lines[1].text.text = "Double tokens";
            lines[1].value.text = Main.Player.Data.Tokens.ToString();
            lines[3].text.text = "Acquired"; 
            lines[3].value.text = Main.Player.GotStationLicense(station) ? "Yes" : "No";
            int shunts = Main.Player.Data.Shunts[order];
            lines[4].text.text = "Shunting";
            lines[4].value.text = shunts + " / " + Main.Player.Config.ShuntThreshold[order];
            int freights = Main.Player.Data.Freights[order];
            lines[5].text.text = "Freight";
            lines[5].value.text = freights + " / " + Main.Player.Config.FreightThreshold[order];
            lines[6].text.text = "Victory";
            lines[6].value.text = Main.Player.Config.VictoryCondition is VictoryCond.NbOfJobs
                ? shunts + freights >= Main.Player.Config.VictoryThreshold
                    ? "Done"
                    : $"{shunts + freights} / {Main.Player.Config.VictoryThreshold}"
                : "N/A";
        }
    }
    
    public override void Disable() {
        foreach (TextMeshPro component in _mainScreen.fees.transform.parent.gameObject
                     .GetComponentsInChildren<TextMeshPro>()) {
            if (component.name.StartsWith("line") || component.name.StartsWith("value"))
                component.text = string.Empty;
        }
    }
    
    public override void HandleInputAction(InputAction input) {
        if (input is InputAction.Cancel or InputAction.Confirm) {
            _mainScreen.screenSwitcher.SetActiveDisplay(_mainScreen);
        }
    }
    
}

[HarmonyPatch(typeof(CareerManagerMainScreen))]
public class CareerManagerMainScreenPatch {
    
    [HarmonyPrefix, HarmonyPatch(nameof(CareerManagerMainScreen.Awake))]
    public static bool Awake_Prefix(CareerManagerMainScreen __instance) {
        if (!Main.IsConnected) return true;
        TextMeshPro randomizerInfos = __instance.fees.transform.parent.gameObject.GetComponentsInChildren<TextMeshPro>()
            .Single(tmPro => tmPro.name == "line8");
        __instance.activeSlotCount = 5;

        GameObject go = new();
        go.transform.SetParent(__instance.feesScreen.transform.parent, false);
        go.AddComponent<CareerManagerRandomizerInfos>();

        randomizerInfos.text = "Randomizer infos";
        
        __instance.selector = new IntIterator(0, 5, true);
        __instance.selectableText = [
            __instance.fees,
            __instance.licenses,
            __instance.ownedVehicles,
            __instance.stats,
            randomizerInfos
        ];
        
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(CareerManagerMainScreen.Activate))]
    public static void Activate_Prefix(CareerManagerMainScreen __instance) {
        if (Main.IsConnected) __instance.fees.transform.parent.gameObject.GetComponentsInChildren<TextMeshPro>()
            .Single(tmPro => tmPro.name == "line8").text = "Randomizer infos";
    }

    [HarmonyPrefix, HarmonyPatch(nameof(CareerManagerMainScreen.Disable))]
    public static void Disable_Prefix(CareerManagerMainScreen __instance) {
        if (Main.IsConnected) __instance.fees.transform.parent.gameObject.GetComponentsInChildren<TextMeshPro>()
            .Single(tmPro => tmPro.name == "line8").text = string.Empty;
    }
    
    [HarmonyPrefix, HarmonyPatch(nameof(CareerManagerMainScreen.HandleInputAction))]
    public static bool HandleInputAction_Prefix(CareerManagerMainScreen __instance, InputAction input) {
        if (!Main.IsConnected) return true;
        if (__instance.selector.Current != 4 || input is not InputAction.Confirm) return true;
        __instance.screenSwitcher.SetActiveDisplay(
            __instance.transform.parent.GetComponentInChildren<CareerManagerRandomizerInfos>()
            );
        return false;
    }
}
