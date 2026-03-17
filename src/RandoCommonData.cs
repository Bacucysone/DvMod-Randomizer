using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archipelago.MultiClient.Net.DataPackage;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using DV.Booklets;
using DV.InventorySystem;
using DV.LocoRestoration;
using DV.Shops;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using UnityEngine;

namespace DvMod.Randomizer
{
    public static class RandoCommonData {
        public static class AP_ID {
            public static long ITEMS = 0x100;
            public static long SLICENSES = 0x200;
            public static long GLICENSES = 0x300;
            public static long JLICENSES = 0x310;
            public static long RELIC = 0x350;
            public static long GARAGES = 0x360;
            public static long LOC_RELIC_PARTS = 0x620;
            public static long LOC_RELIC_PAINTED = 0x630;
            public static long LOC_GENERAL_LICENSES = 0x660;
            public static long LOC_JOB_LICENSES = 0x670;

        }
        public static Sprite GetStationSprite(string name) {
            if (name.Equals("HMB")) name = "HB";
            else if (name.Equals("MFMB")) name = "MF";
            Texture2D icon = new(4,4);
            ImageConversion.LoadImage(icon, File.ReadAllBytes(Path.Combine(Main.mod!.Path,"icons", $"icon_{name}.png")));
            return Sprite.Create(icon, new(0,0,256,256), new(0.5f, 0.5f));
        }
        private static readonly Dictionary<string, int> StationOrder = new() {{"CME", 0}, {"CMS",1}, {"CP",2}, {"CS",3}, {"CW",4}, {"FF",5}, {"FM",6}, {"FRC",7}, {"FRS",8}, {"GF",9}, {"HB",10}, {"IME",11}, {"IMW",12}, {"MB",13}, {"MF",14}, {"OR",15}, {"OWC",16}, {"OWN",17}, {"SM",18}, {"SW",19}};
        private static readonly Dictionary<TrainCarType, int> TrainTypeOrder = new() {
            {TrainCarType.LocoShunter, 0}, 
            {TrainCarType.LocoDM3, 1}, 
            {TrainCarType.LocoDH4, 2}, 
            {TrainCarType.LocoDiesel, 3}, 
            {TrainCarType.LocoS060, 4}, 
            {TrainCarType.LocoSteamHeavy, 5},
            {TrainCarType.Tender, 5}
            };
        public static T? FindMin<T>(this IEnumerable<T> list, Func<T, float> fDist) {
            T elem = list.FirstOrDefault();
            float val = float.PositiveInfinity;
            foreach (T x in list) {
                if (fDist(x) < val) {
                    val = fDist(x);
                    elem = x;
                }
            }
            return elem;
        }
        public static T[] CopyLast<T>(this T[] list) {
            if (list.Count() == 0) return [];
            return [.. list, list.Last()];
        }
        public static string GetFromFlags(ItemFlags flags) {
            if (flags.HasFlag(ItemFlags.Advancement)) 
                return "!!";
            if (flags.HasFlag(ItemFlags.NeverExclude))
                return "!";
            if (flags.HasFlag(ItemFlags.Trap))
                return "...";
            return "";
        }
        public static string GetLocoNameFromType(TrainCarType carType) {
            return carType switch {
                TrainCarType.LocoDH4 => "DH4",
                TrainCarType.LocoSteamHeavy => "S282",
                TrainCarType.LocoS060 => "S060",
                TrainCarType.LocoDiesel => "DE6",
                TrainCarType.LocoDM3 => "DM3",
                TrainCarType.LocoShunter => "DE2",
                _ => "Unknown locomotive"
            };
        }
        public readonly struct SpawnPoint(string n, float x, float y, float z)
        {
            public string Name { get; } = n;
            public Vector3 Position { get; } = new Vector3(x, y, z);
        }
        private static readonly List<SpawnPoint> AddressToLocoRestorationLocation = [
    		new SpawnPoint("CP Shed / A6S", 2216.55f, 145.119f, 9034.95f),
    		new SpawnPoint("CME green building", 15632.13f, 204.28f, 11162.54f),
    		new SpawnPoint("CP Shed / A4S", 1852.979f, 145.119f, 9329.24f),
    		new SpawnPoint("CP / A6S North", 2160.51f, 145.119f, 9042.45f),
    		new SpawnPoint("CMS / A2L", 8514.341f, 156.3079f, 3552.408f),
    		new SpawnPoint("SM Service Shed", 8038.72f, 131.86f, 7127.34f),
    		new SpawnPoint("GF Loco Spawn Shed right", 13087.57f, 140.093f, 11039.52f),
    		new SpawnPoint("IME / A1L", 15170.11f, 248.2943f, 15437.31f),
    		new SpawnPoint("HB Loco Spawn", 12923.64f, 113.08f, 3639.59f),
    		new SpawnPoint("HB D yard Shed", 13518.51f, 112.97f, 3495.79f),
    		new SpawnPoint("IMW / B8L North", 2113.4f, 133.69f, 13433.45f),
    		new SpawnPoint("GF /A3S", 13176.75f, 140.093f, 11059.94f),
    		new SpawnPoint("CP / A6S South", 2253.34f, 145.119f, 8853.61f),
    		new SpawnPoint("FRS / B1L", 5325.05f, 174.74f, 3785.03f),
    		new SpawnPoint("SM / A6I", 7925.49f, 131.86f, 7188.08f),
    		new SpawnPoint("HB Shop", 13427.02f, 112.97f, 3622.94f),
    		new SpawnPoint("FF B yard", 9521.979f, 119.2f, 13465.91f),
    		new SpawnPoint("GF South exit", 12582.31f, 110.51f, 10648.67f),
    		new SpawnPoint("CW Plaza B yard", 1862.01f, 122.323f, 5450.5f),
    		new SpawnPoint("HB Roundhouse", 12788.12f, 113.08f, 3601.81f),
    		new SpawnPoint("OWC / A1L", 4929.6f, 122.96f, 6324.2f),
    		new SpawnPoint("CP / A4S", 1856.359f, 145.119f, 9288.9f),
    		new SpawnPoint("SM / A4S", 7924.46f, 131.86f, 7112.42f),
    		new SpawnPoint("SW / C1O Shed", 1309.609f, 147.27f, 2193.77f),
    		new SpawnPoint("GF / C1SP", 13021.11f, 140.093f, 11083.36f),
    		new SpawnPoint("HB / F4SP", 13380.01f, 112.97f, 3542.92f),
    		new SpawnPoint("FF C yard between buildings", 9400.66f, 120.8f, 13476.36f),
    		new SpawnPoint("GF Loco Spawn Shed left", 13066.43f, 140.093f, 11023.47f),
    		new SpawnPoint("MF Roundhouse East", 2212.609f, 159.193f, 10615.77f),
    		new SpawnPoint("OWN Service Shed", 11535.71f, 122.24f, 11628.09f),
    		new SpawnPoint("CW / C6L", 1823.676f, 122.213f, 5664.788f),
    		new SpawnPoint("CS / A1LP", 10017.58f, 134.73f, 1378.58f),
    		new SpawnPoint("OR / A4S", 6552.149f, 143.92f, 11473.41f),
    		new SpawnPoint("CW/OWC middle triangle", 3320.215f, 112.935f, 5688.702f),
    		new SpawnPoint("CW NE of B yard", 1924.729f, 122.213f, 5567.21f),
    		new SpawnPoint("SM / A3S", 7917.89f, 131.73f, 7247.16f),
    		new SpawnPoint("FM / A3L", 6007.85f, 123.89f, 6639.3f),
    		new SpawnPoint("IMW / B8L South", 2193.83f, 133.69f, 13333.51f),
    		new SpawnPoint("OR / A6S", 6568.97f, 143.92f, 11452.62f),
    		new SpawnPoint("FF / D1L", 9369.199f, 120.78f, 13418.32f),
    		new SpawnPoint("FF Service shed", 9327.21f, 119.2f, 13358.35f),
    		new SpawnPoint("CMS brick building", 8498.551f, 156.3079f, 3233.249f),
    		new SpawnPoint("FF Turntable", 9381.989f, 119.2f, 13330.21f),
    		new SpawnPoint("CS Museum", 10274.72f, 134.73f, 1443.29f),
    		new SpawnPoint("IMW SE of Office", 2185.58f, 133.69f, 13195.64f),
    		new SpawnPoint("CP / A1S", 2004.42f, 145.119f, 8912.18f),
    		new SpawnPoint("HB D yard shed", 13279.06f, 112.97f, 3437.66f),
    		new SpawnPoint("SM W/ A7L 1", 7848.25f, 131.73f, 7213.14f),
    		new SpawnPoint("HB F yard East", 13764.49f, 112.97f, 3556.25f),
    		new SpawnPoint("OR / B7S", 6394.12f, 143.92f, 11365.58f),
    		new SpawnPoint("OR / A3S", 6452.59f, 143.92f, 11230.71f),
    		new SpawnPoint("SM / A7L 2", 7863.1f, 131.73f, 7208.11f),
    		new SpawnPoint("MF Roundhouse East 2", 2278.24f, 159.193f, 10676.87f),
    		new SpawnPoint("FRC C yard North", 5759.84f, 144.91f, 9003.39f),
    		new SpawnPoint("CW East exit", 2243.05f, 111.01f, 5699.65f),
    		new SpawnPoint("CME Coal Mine", 15552.81f, 181.5f, 11033.37f),
    		new SpawnPoint("MF Roundhouse West", 2267.709f, 159.193f, 10657.35f)
    	];
        private static readonly string[] AddressToItemName = ["AmpLimiter",
            "AntiWheelslipComputer",
            "Ashtray",
            "AutomaticTrainStop",
            "Banknotes",
            "BatteryCharger",
            "BeaconAmber",
            "BeaconBlue",
            "BeaconRed",
            "Boombox",
            "BottleMilk",
            "BottlePlastic",
            "BoxCardboard_open",
            "BrakeChecklist",
            "BrakeCylinderLEDBar",
            "BrokenLabel",
            "Calculator",
            "CanisterFuel",
            "CanisterGas",
            "Cassette_Album01",
            "Cassette_Album02",
            "Cassette_Album03",
            "Cassette_Album04",
            "Cassette_Album05",
            "Cassette_Album06",
            "Cassette_Album07",
            "Cassette_Album08",
            "Cassette_Album09",
            "Cassette_Album10",
            "Cassette_Album11",
            "Cassette_Album12",
            "Cassette_Album13",
            "Cassette_Album14",
            "Cassette_Album15",
            "Cassette_Album16",
            "Cassette_Playlist01",
            "Cassette_Playlist02",
            "Cassette_Playlist03",
            "Cassette_Playlist04",
            "Cassette_Playlist05",
            "Cassette_Playlist06",
            "Cassette_Playlist07",
            "Cassette_Playlist08",
            "Cassette_Playlist09",
            "Cassette_Playlist10",
            "Clinometer",
            "Clipboard",
            "CoalLump1",
            "CoalLump2",
            "CoalLump3",
            "CoffeePot",
            "Coin",
            "CoinSquished",
            "CommsRadio",
            "Compass",
            "Crate",
            "CratePlastic",
            "CrimpingTool",
            "Cup1",
            "Cup2",
            "DebtWarningReport",
            "DefectDetector",
            "DigitalClock",
            "DigitalSpeedometer",
            "DistanceTracker",
            "DuctTape",
            "DuctTapeEmpty",
            "ElectricStove",
            "EOTLantern",
            "Eraser",
            "ExpertShovel",
            "FeesReport",
            "FillerGun",
            "FireExtinguisher",
            "FlagMarkerBlue",
            "FlagMarkerCyan",
            "FlagMarkerGreen",
            "FlagMarkerOrange",
            "FlagMarkerPurple",
            "FlagMarkerRed",
            "FlagMarkerWhite",
            "FlagMarkerYellow",
            "Flashlight",
            "GoldenShovel",
            "GooglyEye",
            "Hammer",
            "HandDrill",
            "HandheldGameConsole",
            "Hanger",
            "Hat",
            "Headlight",
            "InfraredThermometer",
            "ItemContainerBriefcase",
            "ItemContainerCrate",
            "ItemContainerFolder",
            "ItemContainerFolderBlue",
            "ItemContainerFolderRed",
            "ItemContainerFolderYellow",
            "ItemContainerRegistrator",
            "ItemContainerToolbox",
            "JobBooklet",
            "JobExpiredReport",
            "JobMissingLicenseReport",
            "JobOverview",
            "JobReport",
            "Key",
            "Keyboard",
            "KeyCaboose",
            "KeyDE6Slug",
            "KeyDM1U",
            "Label",
            "LabelMaker",
            "Lamp",
            "Lantern",
            "LaptopOpen",
            "LicenseConcurrentJobs1",
            "LicenseConcurrentJobs1Info",
            "LicenseConcurrentJobs2",
            "LicenseConcurrentJobs2Info",
            "LicenseDispatcher1",
            "LicenseDispatcher1Info",
            "LicenseFragile",
            "LicenseFragileInfo",
            "LicenseFreightHaul",
            "LicenseFreightHaulInfo",
            "LicenseHazmat1",
            "LicenseHazmat1Info",
            "LicenseHazmat2",
            "LicenseHazmat2Info",
            "LicenseHazmat3",
            "LicenseHazmat3Info",
            "LicenseLocomotiveDE2",
            "LicenseLocomotiveDE2Info",
            "LicenseLocomotiveDE6",
            "LicenseLocomotiveDE6Info",
            "LicenseLocomotiveDH4",
            "LicenseLocomotiveDH4Info",
            "LicenseLocomotiveDM3",
            "LicenseLocomotiveDM3Info",
            "LicenseLocomotiveS060",
            "LicenseLocomotiveS060Info",
            "LicenseLocomotiveSH282",
            "LicenseLocomotiveSH282Info",
            "LicenseLogisticalHaul",
            "LicenseLogisticalHaulInfo",
            "LicenseManualService",
            "LicenseManualServiceInfo",
            "LicenseMilitary1",
            "LicenseMilitary1Info",
            "LicenseMilitary2",
            "LicenseMilitary2Info",
            "LicenseMilitary3",
            "LicenseMilitary3Info",
            "LicenseMultipleUnit",
            "LicenseMultipleUnitInfo",
            "LicenseMuseumCitySouth",
            "LicenseMuseumCitySouthInfo",
            "LicenseShunting",
            "LicenseShuntingInfo",
            "LicenseTrainDriver",
            "LicenseTrainDriverInfo",
            "LicenseTrainLength1",
            "LicenseTrainLength1Info",
            "LicenseTrainLength2",
            "LicenseTrainLength2Info",
            "LightBarBlue",
            "LightBarCyan",
            "LightBarGreen",
            "LightBarOrange",
            "LightBarPurple",
            "LightBarRed",
            "LightBarWhite",
            "LightBarYellow",
            "lighter",
            "LockerKey",
            "Map",
            "MapSchematic",
            "MarkerPen",
            "ModernHeadlightL",
            "ModernHeadlightR",
            "ModernTaillightL",
            "ModernTaillightR",
            "Mount70Long",
            "Mount90Square",
            "Mount90SquareBig",
            "Mount90SquareLong",
            "Mount90Wide",
            "MountLong",
            "MountSmall",
            "MountSquare",
            "MountSquareBig",
            "MountSquareVeryLong",
            "MountStandBig",
            "MountVeryLong",
            "Mouse",
            "Mug",
            "Nameplate",
            "Oiler",
            "OverheatingProtection",
            "PaintCan",
            "PaintCan_Museum",
            "PaintCan_Sand",
            "PaintCanOpen",
            "PaintCanOpen_Museum",
            "PaintCanOpen_Sand",
            "PaintSprayer",
            "Paper",
            "PaperBox",
            "PaperBoxCap",
            "Pen",
            "Pencil",
            "ProximityReader",
            "ProximitySensor",
            "ReceiptBooklet",
            "Registrator",
            "RemoteController",
            "RemoteSignalBooster",
            "RouteMap",
            "Ruler",
            "Scanner",
            "ShelfSmall",
            "shovel",
            "ShovelMount",
            "SolderingGun",
            "SolderingWireReel",
            "SolderingWireReelEmpty",
            "SteamEngineChecklist",
            "StickyTape",
            "Stopwatch",
            "SunVisor",
            "SwitchAlternating",
            "SwitchAnalog",
            "SwitchButton",
            "SwitchLever",
            "SwitchRotary",
            "SwitchSetter",
            "SwivelLight",
            "TableFan",
            "Taillight",
            "Trashbin",
            "TutorialSummary",
            "TutorialWarningReport",
            "UniversalControlStand",
            "VehicleCatalog",
            "wallet",
            "WirelessMUController"];
        private static readonly Dictionary<JobLicenses, int> IdToJobLocations = new () {
            {JobLicenses.Shunting, 0},
            {JobLicenses.LogisticalHaul, 1},
            {JobLicenses.Fragile, 2},
            {JobLicenses.TrainLength1, 3},
            {JobLicenses.TrainLength2, 4},
            {JobLicenses.Hazmat1, 5},
            {JobLicenses.Hazmat2, 6},
            {JobLicenses.Hazmat3, 7},
            {JobLicenses.Military1, 8},
            {JobLicenses.Military2, 9},
            {JobLicenses.Military3, 10},
            {JobLicenses.Basic, -1},
            {JobLicenses.FreightHaul, 11}
        };
        private static readonly Dictionary<GeneralLicenseType, int> IdToGeneralLocations = new () {
            {GeneralLicenseType.DM3, 1},
            {GeneralLicenseType.DH4, 2},
            {GeneralLicenseType.DE6, 3},
            {GeneralLicenseType.S060, 4},
            {GeneralLicenseType.SH282, 5},
            {GeneralLicenseType.ManualService, 6},
            {GeneralLicenseType.MultipleUnit, 7},
            {GeneralLicenseType.ConcurrentJobs1, 8},
            {GeneralLicenseType.ConcurrentJobs2, 9},
            {GeneralLicenseType.MuseumCitySouth, 10},
            {GeneralLicenseType.Dispatcher1, 11},
            {GeneralLicenseType.DE2, 0},
            {GeneralLicenseType.TrainDriver, 12},
            {GeneralLicenseType.NotSet, -3}
        };
        private readonly struct ShopLocationEntry(string n, Vector3 p) {
            public string Name {get;} = n;
            public Vector3 Position {get;} = p;
        }
        private static readonly List<ShopLocationEntry> allShops = [
            new("shop_MF", new(2232.3f,159.3f,10833.6f)),
            new("shop_CW", new(1915.9f,122.3f,5784.7f)),
            new("shop_FF", new(9533.7f,119.3f,13419.2f)),
            new("shop_HB", new(13423f,113.1f,3617.3f)),
            new("shop_GF", new(13032.2f,140.2f,11163.5f))
        ];
        public static string GetNearestShop(Vector3 position) {
            string shop = "";
            float distance = float.PositiveInfinity;
            foreach (ShopLocationEntry entry in allShops) {
                if ((position-entry.Position).magnitude < distance) {
                    shop = entry.Name;
                }
            }
            return shop.Substring(5);
        }
        private static List<string> UniqueItems = ["AmpLimiter",
            "AntiWheelslipComputer",
            "AutomaticTrainStop",
            "BatteryCharger",
            "BeaconAmber",
            "BeaconBlue",
            "BeaconRed",
            "Boombox",
            "Cassette_Album01",
            "Cassette_Album02",
            "Cassette_Album03",
            "Cassette_Album04",
            "Cassette_Album05",
            "Cassette_Album06",
            "Cassette_Album07",
            "Cassette_Album08",
            "Cassette_Album09",
            "Cassette_Album10",
            "Cassette_Album11",
            "Cassette_Album12",
            "Cassette_Album13",
            "Cassette_Album14",
            "Cassette_Album15",
            "Cassette_Album16",
            "Cassette_Playlist01",
            "Cassette_Playlist02",
            "Cassette_Playlist03",
            "Cassette_Playlist04",
            "Cassette_Playlist05",
            "Cassette_Playlist06",
            "Cassette_Playlist07",
            "Cassette_Playlist08",
            "Cassette_Playlist09",
            "Cassette_Playlist10",
            "Clinometer",
            "CrimpingTool",
            "ExpertShovel",
            "DefectDetector",
            "DigitalClock",
            "DigitalSpeedometer",
            "DistanceTracker",
            "FillerGun",
            "FlagMarkerBlue",
            "FlagMarkerCyan",
            "FlagMarkerGreen",
            "FlagMarkerOrange",
            "FlagMarkerPurple",
            "FlagMarkerRed",
            "FlagMarkerWhite",
            "FlagMarkerYellow",
            "Flashlight",
            "GoldenShovel",
            "GooglyEye",
            "HandDrill",
            "HandheldGameConsole",
            "Headlight",
            "InfraredThermometer",
            "ItemContainerBriefcase",
            "ItemContainerCrate",
            "ItemContainerFolder",
            "ItemContainerFolderBlue",
            "ItemContainerFolderRed",
            "ItemContainerFolderYellow",
            "ItemContainerRegistrator",
            "ItemContainerToolbox",
            "Key",
            "KeyCaboose",
            "KeyDE6Slug",
            "KeyDM1U",
            "LabelMaker",
            "LightBarBlue",
            "LightBarCyan",
            "LightBarGreen",
            "LightBarOrange",
            "LightBarPurple",
            "LightBarRed",
            "LightBarWhite",
            "LightBarYellow",
            "ModernHeadlightL",
            "ModernHeadlightR",
            "ModernTaillightL",
            "ModernTaillightR",
            "OverheatingProtection",
            "PaintCan_Museum",
            "ProximityReader",
            "ProximitySensor",
            "Stopwatch",
            "SunVisor",
            "UniversalControlStand"];

        private static readonly JobLicenses[][] IdToJobLicense = [[JobLicenses.FreightHaul], [JobLicenses.LogisticalHaul], [JobLicenses.Shunting], [JobLicenses.Fragile], [JobLicenses.TrainLength1, JobLicenses.TrainLength2], [JobLicenses.Hazmat1, JobLicenses.Hazmat2, JobLicenses.Hazmat3], [JobLicenses.Military1, JobLicenses.Military2, JobLicenses.Military3]];
        private static readonly GeneralLicenseType[][] IdToGeneralLicense = [[GeneralLicenseType.Dispatcher1], [GeneralLicenseType.TrainDriver], [GeneralLicenseType.DE2], [GeneralLicenseType.DM3], [GeneralLicenseType.DH4], [GeneralLicenseType.DE6], [GeneralLicenseType.S060], [GeneralLicenseType.SH282], [GeneralLicenseType.MultipleUnit], [GeneralLicenseType.MuseumCitySouth], [GeneralLicenseType.ManualService], [GeneralLicenseType.ConcurrentJobs1, GeneralLicenseType.ConcurrentJobs2]];

        public static string GetStationNameFromId(long id) {
            long Order = id - AP_ID.SLICENSES;
            foreach (KeyValuePair<string, int> item in StationOrder) {
                if (item.Value == Order) return item.Key;
            }
            return "";
        }
        public static int GetOrderFromStationName(string name) {
            return StationOrder[name];
        }
        public static TrainCarType GetCarTypeFromID(long queryId) {
            foreach (KeyValuePair<TrainCarType, int> value in TrainTypeOrder) {
                if (value.Value == queryId) return value.Key;
            }
            return TrainCarType.NotSet;
        }
        public static int GetOrderFromLocoType(TrainCarType carType) {
            if (carType == TrainCarType.LocoMicroshunter) carType = TrainCarType.LocoShunter;
            else if (carType == TrainCarType.LocoDM1U) carType = TrainCarType.LocoDM3;
            return TrainTypeOrder[carType];
        }
        public static GeneralLicenseType_v2[] GetGeneralLicenseFromId(long id) {
            try {
                return [.. IdToGeneralLicense[id-AP_ID.GLICENSES].Select(l => l.ToV2())];
            } catch (IndexOutOfRangeException) {
                return [];
            }
        }
        public static int GetIDFromJobLicense(JobLicenseType_v2 jobLicense) {
            return IdToJobLocations[jobLicense.v1];
        }
        
        public static JobLicenseType_v2[] GetJobLicenseFromId(long id) {
            try {
                return [.. IdToJobLicense[id-AP_ID.JLICENSES].Select(l => l.ToV2())];
            } catch (IndexOutOfRangeException) {
                return [];
            }
        }
        public static int GetIDFromGeneralLicense(GeneralLicenseType_v2 generalLicense) {
            return IdToGeneralLocations[generalLicense.v1];
        }
        public static string GetItemPrefabFromId(long id) {
            return AddressToItemName[id - AP_ID.ITEMS];
        }
        public static string GetRelicNameFromId(long id) {
            return GetCarTypeFromID(id-AP_ID.RELIC) switch {
                TrainCarType.LocoShunter => "DE2",
                TrainCarType.LocoSteamHeavy => "S282",
                TrainCarType.LocoS060 => "S060",
                TrainCarType.LocoDiesel => "DE6",
                TrainCarType.LocoDM3 => "DM3",
                TrainCarType.LocoDH4 => "DH4",
                _ => "ERROR"
            };
        }
        public static LocoRestorationController GetLocoControllerFromId(long id) {
            return LocoRestorationController.allLocoRestorationControllers.Find(cont => cont.loco.carType == GetCarTypeFromID(id-AP_ID.RELIC));
        }
        public static LocoRestorationController.RestorationState GetState(TrainCarType carType) {
            if (LocoRestorationController.allLocoRestorationControllers == null) return LocoRestorationController.RestorationState.S0_Initialized;
            return LocoRestorationController.allLocoRestorationControllers.Find(cont => cont.locoLivery.v1 == carType).State;
        }
        
        public static int GetIdFromLocoLocations(Vector3 position) {
            return AddressToLocoRestorationLocation.FindIndex(sp => sp.Position == position);
        }
        public static string GetStationFromLocoLocations(Vector3 position) {
            SpawnPoint sPoint = AddressToLocoRestorationLocation.FindMin(sp => (sp.Position - position).magnitude);
            int n = (sPoint.Name[2] == '/' || sPoint.Name[2] == ' ')?2:3;
            return sPoint.Name.Substring(0, n);
        }
        public static (Vector3, float) GetClosestLocoLocation(Vector3 position) {
            Vector3 Pos = new(0.0f,0.0f,0.0f);
            float Distance = float.PositiveInfinity;
            foreach (SpawnPoint sPoint in AddressToLocoRestorationLocation) {
                if ((sPoint.Position - position).magnitude < Distance){
                    Pos = sPoint.Position;
                    Distance = (sPoint.Position - position).magnitude;
                }
            }
            return (Pos, Distance);
        }
        public static string GetNameFromGarageID(long id) {
            return id switch {
                0x360 => "BE2",
                0x361 => "Caboose",
                0x362 => "DE6 Slug",
                0x363 => "DM1U",
                _ => throw new ArgumentException("Asked for garage name but is not a garage ID")
            };
        }
        private static CashRegisterModule.CashRegisterModuleData GetFreightData(string name, int idx) {
            return new() {
                unitsToBuy=idx+1,
                pricePerUnit=0,
                resourceName="Freight "+(idx+1)+": "+Main.player!.GetItemNameFromLocationId(0x2500+GetOrderFromStationName(name)*0x100+idx, true),
                resourceIcon= TrainCarType.LocoDiesel.ToV2().icon,
                car=null
            };
        }
        private static CashRegisterModule.CashRegisterModuleData GetShuntingData(string name, int idx) {
            return new() {
                unitsToBuy=idx+1,
                pricePerUnit=0,
                resourceName="Shunting "+(idx+1)+": "+Main.player!.GetItemNameFromLocationId(0x1000+GetOrderFromStationName(name)*0x100+idx, true),
                resourceIcon= TrainCarType.LocoShunter.ToV2().icon,
                car=null
            };
        }
        public static List<CashRegisterModule.CashRegisterModuleData> GetStationLicenseData(string name) {
            List<CashRegisterModule.CashRegisterModuleData> StationLicense = [new() {
                unitsToBuy=1,
                pricePerUnit=0,
                resourceName=name+" station license",
                resourceIcon=GetStationSprite(name),
                car=null
            }];
            return StationLicense;
            
        }
        public static void AcquireStationLicense(string name) {
            GameObject license = BookletCreator_CashRegisterReceipt.Create(GetStationLicenseData(name), Main.player!.Position, Main.player!.Rotation, WorldMover.OriginShiftParent);
            license.name=name+"SL";
            InventoryItemSpec item = license.GetComponent<InventoryItemSpec>();
            item.BelongsToPlayer = true;
            item.name=name+" station license";
            SingletonBehaviour<StorageController>.Instance.AddItemToWorldStorageAfterOneFrame(license);
        }
        public static DV_APItem GetAPItem(int idx, ItemInfo item) {
            return item.ItemId switch {
                -1 => new AP_Nothing(idx, item),
                1 => new AP_Money(idx, item),
                >= 0x100 and < 0x200 => new AP_PhysicalItem(idx, item),
                >= 0x200 and < 0x300 => new AP_StationLicense(idx, item),
                >= 0x300 and < 0x320 => new AP_GameLicense(idx, item),
                >= 0x350 and < 0x360 => new AP_RelicLoco(idx, item),
                >= 0x360 and < 0x370 => new AP_CrewVehicle(idx, item),
                _ => throw new ArgumentException("Invalid item id")
            };
        }
        public static DV_APItem GetAPItem(long id) {
            ItemInfo item = new(new(){Item = id, Location = -1, Player = -1},"Derail Valley", "Derail Valley",Main.player!.Session.Items.ItemInfoResolver,new(){});
            return GetAPItem(-1, item);
        }
        public static GarageType_v2 GetGarageFromId(long Id) {
            return Id switch {
                0x360 => Garage.Bob.ToV2(),
                0x361 => Garage.Caboose.ToV2(),
                0x362 => Garage.DE6_Slug.ToV2(),
                0x363 => Garage.DM1U.ToV2(),
                _ => throw new ArgumentException("GetGarageFromId: Id is not a Garage")
            };
        }
    }

}