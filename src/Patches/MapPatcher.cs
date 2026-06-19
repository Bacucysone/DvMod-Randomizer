using System.Collections;
using DV.Teleporters;
using DV.Utils;
using HarmonyLib;
using UnityEngine;

namespace DvMod.Randomizer {
    [HarmonyPatch(typeof(MapMarker), nameof(MapMarker.Init))]
    public static class MapMarkerPatcher {
        private static Color GotLicenseColor => new(0f,3f,0f);
        private static Color NoLicenseColor => new(3f,0f,0f);
        private static readonly MapMarker[] AllMarkers = new MapMarker[20];
        private static readonly int ColorStringId = Shader.PropertyToID("_Color");

        public static void Postfix(MapMarker __instance) {
            if (!Main.IsConnected ||
                __instance.fastTravelDestination.markerType != FastTravelDestination.MarkerType.Station) return;
            string stationName = StationController.allStations.FindMin(
                sc => Vector3.Distance(sc.stationRange.stationCenterAnchor.position, __instance.fastTravelDestination.playerTeleportAnchor.position)
            )!.stationInfo.YardID;
            int order = RandoCommonData.GetOrderFromStationName(stationName);
            AllMarkers[order] = __instance;
            if (Main.Player.GotStationLicense(stationName))
                GotLicense(stationName);
            else
                NoLicense(stationName);
        }
        private static IEnumerator ChangeMarkerColor(int order, Color color) {
            while (AllMarkers[order]==null) yield return null;
            MeshRenderer renderer = AllMarkers[order].GetComponentInChildren<MeshRenderer>();
            renderer.material.SetColor(ColorStringId, color);
        }
        public static void GotLicense(string stationName) => SingletonBehaviour<CoroutineManager>.Instance.Run(ChangeMarkerColor(RandoCommonData.GetOrderFromStationName(stationName), GotLicenseColor));
        public static void NoLicense(string stationName) => SingletonBehaviour<CoroutineManager>.Instance.Run(ChangeMarkerColor(RandoCommonData.GetOrderFromStationName(stationName), NoLicenseColor));
    }
}