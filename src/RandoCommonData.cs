using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using DV.Booklets;
using DV.CabControls;
using DV.LocoRestoration;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using UnityEngine;

namespace DvMod.Randomizer;

public static class RandoCommonData {

    /*private readonly struct ShopLocationEntry(string n, Vector3 p) {
        public string Name {get;} = n;
        public Vector3 Position {get;} = p;
    }
    private static readonly List<ShopLocationEntry> allShops = [
        new("shop_MF", new Vector3(2232.3f,159.3f,10833.6f)),
        new("shop_CW", new Vector3(1915.9f,122.3f,5784.7f)),
        new("shop_FF", new Vector3(9533.7f,119.3f,13419.2f)),
        new("shop_HB", new Vector3(13423f,113.1f,3617.3f)),
        new("shop_GF", new Vector3(13032.2f,140.2f,11163.5f))
    ];
    public static string GetNearestShop(Vector3 position) {
        return allShops.FindMin(entry => (position-entry.Position).magnitude).Name.Substring(5)
    }

    private static readonly List<string> UniqueItems = ["AmpLimiter",
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
        "UniversalControlStand"]; */

    #region Mappings DV Items/Events -> long id Locations
    
    private const long LOC_RELIC_PARTS = 0x620;
    private const long LOC_RELIC_PAINTED = 0x630;
    private const long LOC_GENERAL_LICENSES = 0x660;
    private const long LOC_JOB_LICENSES = 0x670;
    private const long LOC_LOCO_RESTORATION = 0x400;
    private const long LOC_LOCO_NB_JOBS = 0x600;
    private const long LOC_GARAGE_UNLOCKED = 0x690;
    
    private static readonly string[] StationOrder = [
        "CME", "CMS", "CP", "CS", "CW", "FF", "FM", "FRC", "FRS", "GF", "HB", "HMB", "IME", "IMW", "MB", "MF", "MFMB", "OR", "OWC", "OWN", "SM", "SW"
    ];
    public static string GetStationNameFromOrder(int order) => StationOrder.ElementAtOrDefault(order);
    public static int GetOrderFromStationName(string name) => Array.IndexOf(StationOrder, name);
    
    public static long GetLocoNbJobsIdFromOrder(int order) => order + LOC_LOCO_NB_JOBS;
    
    public static long GetRelicPaintedIdFromLoco(TrainCarType carType) => GetOrderFromLocoType(carType) + LOC_RELIC_PAINTED;
    public static long GetRelicPartsToMuseumIdFromLoco(TrainCarType carType) => GetOrderFromLocoType(carType) + LOC_RELIC_PARTS;
    
    private static readonly TrainCarType[] TrainTypeOrder = [
        TrainCarType.LocoShunter, 
        TrainCarType.LocoDM3, 
        TrainCarType.LocoDH4, 
        TrainCarType.LocoDiesel, 
        TrainCarType.LocoS060, 
        TrainCarType.LocoSteamHeavy, 
        TrainCarType.Tender
    ];
    public static TrainCarType GetCarTypeFromOrder(int order) => TrainTypeOrder.ElementAtOrDefault(order);
    public static int GetOrderFromLocoType(TrainCarType carType) => Array.IndexOf(TrainTypeOrder, carType);
    
    public static int GetOrderFromLocoLicense(GeneralLicenseType_v2 license) {
        if (license == null) return -1;
        return license.v1 switch {
            GeneralLicenseType.DE2 => 0,
            GeneralLicenseType.DM3 => 1,
            GeneralLicenseType.DH4 => 2,
            GeneralLicenseType.DE6 => 3,
            GeneralLicenseType.S060 => 4,
            GeneralLicenseType.SH282 => 5,
            _ => -1
        };
    }
    
    public static LocoRestorationController.RestorationState GetState(TrainCarType carType) =>
        LocoRestorationController.allLocoRestorationControllers == null ? 
            LocoRestorationController.RestorationState.S0_Initialized : 
            LocoRestorationController.GetForLivery(carType.ToV2()).State;
    
        public readonly struct SpawnPoint(string n, float x, float y, float z)
    {
        public string Name { get; } = n;
        public Vector3 Position { get; } = new(x, y, z);
    }
    private static readonly List<SpawnPoint> AddressToLocoRestorationLocation = [
        new("CP Shed / A6S", 2216.55f, 145.119f, 9034.95f),
        new("CME green building", 15632.13f, 204.28f, 11162.54f),
        new("CP Shed / A4S", 1852.979f, 145.119f, 9329.24f),
        new("CP / A6S North", 2160.51f, 145.119f, 9042.45f),
        new("CMS / A2L", 8514.341f, 156.3079f, 3552.408f),
        new("SM Service Shed", 8038.72f, 131.86f, 7127.34f),
        new("GF Loco Spawn Shed right", 13087.57f, 140.093f, 11039.52f),
        new("IME / A1L", 15170.11f, 248.2943f, 15437.31f),
        new("HB Loco Spawn", 12923.64f, 113.08f, 3639.59f),
        new("HB D yard Shed", 13518.51f, 112.97f, 3495.79f),
        new("IMW / B8L North", 2113.4f, 133.69f, 13433.45f),
        new("GF /A3S", 13176.75f, 140.093f, 11059.94f),
        new("CP / A6S South", 2253.34f, 145.119f, 8853.61f),
        new("FRS / B1L", 5325.05f, 174.74f, 3785.03f),
        new("SM / A6I", 7925.49f, 131.86f, 7188.08f),
        new("HB Shop", 13427.02f, 112.97f, 3622.94f),
        new("FF B yard", 9521.979f, 119.2f, 13465.91f),
        new("GF South exit", 12582.31f, 110.51f, 10648.67f),
        new("CW Plaza B yard", 1862.01f, 122.323f, 5450.5f),
        new("HB Roundhouse", 12788.12f, 113.08f, 3601.81f),
        new("OWC / A1L", 4929.6f, 122.96f, 6324.2f),
        new("CP / A4S", 1856.359f, 145.119f, 9288.9f),
        new("SM / A4S", 7924.46f, 131.86f, 7112.42f),
        new("SW / C1O Shed", 1309.609f, 147.27f, 2193.77f),
        new("GF / C1SP", 13021.11f, 140.093f, 11083.36f),
        new("HB / F4SP", 13380.01f, 112.97f, 3542.92f),
        new("FF C yard between buildings", 9400.66f, 120.8f, 13476.36f),
        new("GF Loco Spawn Shed left", 13066.43f, 140.093f, 11023.47f),
        new("MF Roundhouse East", 2212.609f, 159.193f, 10615.77f),
        new("OWN Service Shed", 11535.71f, 122.24f, 11628.09f),
        new("CW / C6L", 1823.676f, 122.213f, 5664.788f),
        new("CS / A1LP", 10017.58f, 134.73f, 1378.58f),
        new("OR / A4S", 6552.149f, 143.92f, 11473.41f),
        new("CW/OWC middle triangle", 3320.215f, 112.935f, 5688.702f),
        new("CW NE of B yard", 1924.729f, 122.213f, 5567.21f),
        new("SM / A3S", 7917.89f, 131.73f, 7247.16f),
        new("FM / A3L", 6007.85f, 123.89f, 6639.3f),
        new("IMW / B8L South", 2193.83f, 133.69f, 13333.51f),
        new("OR / A6S", 6568.97f, 143.92f, 11452.62f),
        new("FF / D1L", 9369.199f, 120.78f, 13418.32f),
        new("FF Service shed", 9327.21f, 119.2f, 13358.35f),
        new("CMS brick building", 8498.551f, 156.3079f, 3233.249f),
        new("FF Turntable", 9381.989f, 119.2f, 13330.21f),
        new("CS Museum", 10274.72f, 134.73f, 1443.29f),
        new("IMW SE of Office", 2185.58f, 133.69f, 13195.64f),
        new("CP / A1S", 2004.42f, 145.119f, 8912.18f),
        new("HB D yard shed", 13279.06f, 112.97f, 3437.66f),
        new("SM W/ A7L 1", 7848.25f, 131.73f, 7213.14f),
        new("HB F yard East", 13764.49f, 112.97f, 3556.25f),
        new("OR / B7S", 6394.12f, 143.92f, 11365.58f),
        new("OR / A3S", 6452.59f, 143.92f, 11230.71f),
        new("SM / A7L 2", 7863.1f, 131.73f, 7208.11f),
        new("MF Roundhouse East 2", 2278.24f, 159.193f, 10676.87f),
        new("FRC C yard North", 5759.84f, 144.91f, 9003.39f),
        new("CW East exit", 2243.05f, 111.01f, 5699.65f),
        new("CME Coal Mine", 15552.81f, 181.5f, 11033.37f),
        new("MF Roundhouse West", 2267.709f, 159.193f, 10657.35f)
    ];
    public static (Vector3, long) GetInfoRestorationFromLocoLocationOrder(int idx) =>
        (AddressToLocoRestorationLocation[idx].Position, idx + LOC_LOCO_RESTORATION);
    public static int GetIdFromLocoLocations(Vector3 position) =>
        AddressToLocoRestorationLocation.FindIndex(sp => sp.Position == position);
    
    public static string GetStationFromLocoLocations(Vector3 position) {
        SpawnPoint sPoint = AddressToLocoRestorationLocation.FindMin(sp => (sp.Position - position).magnitude);
        int n = (sPoint.Name[2] == '/' || sPoint.Name[2] == ' ')?2:3;
        return sPoint.Name.Substring(0, n);
    }
    
    public static long ComputeCheckForJob(bool isShunting, string station, int nb) {
        long check = 0x2000;
        if (!isShunting)
            check += 0x2000;
        return check + 0x100 * GetOrderFromStationName(station) + nb;
    }
    
    private static readonly JobLicenses[] JobLocationsOrder = [
        JobLicenses.Shunting, 
        JobLicenses.LogisticalHaul, 
        JobLicenses.Fragile, 
        JobLicenses.TrainLength1,
        JobLicenses.TrainLength2, 
        JobLicenses.Hazmat1, 
        JobLicenses.Hazmat2, 
        JobLicenses.Hazmat3, 
        JobLicenses.Military1, 
        JobLicenses.Military2, 
        JobLicenses.Military3,
        JobLicenses.FreightHaul, 
    ];
    public static int GetOrderFromJobLicense(JobLicenseType_v2 jobLicense) => Array.IndexOf(JobLocationsOrder, jobLicense.v1);
    public static long GetIdFromJobLicense(JobLicenseType_v2 jobLicense) {
        int order = GetOrderFromJobLicense(jobLicense);
        return order < 0 ? -1L : order + LOC_JOB_LICENSES;
    }
    public static long GetIdLocoJobsFromOrder(int order) => LOC_LOCO_NB_JOBS + order;
    public static JobLicenseType_v2 GetJobLicenseFromId(long id) =>
        JobLocationsOrder.ElementAtOrDefault(id.Offset(LOC_JOB_LICENSES)).ToV2();
    
    
    private static readonly GeneralLicenseType[] GeneralLocationsOrder = [
        GeneralLicenseType.DE2, 
        GeneralLicenseType.DM3, 
        GeneralLicenseType.DH4, 
        GeneralLicenseType.DE6, 
        GeneralLicenseType.S060,
        GeneralLicenseType.SH282,
        GeneralLicenseType.ManualService,
        GeneralLicenseType.MultipleUnit,
        GeneralLicenseType.ConcurrentJobs1, 
        GeneralLicenseType.ConcurrentJobs2, 
        GeneralLicenseType.MuseumCitySouth, 
        GeneralLicenseType.Dispatcher1,
        GeneralLicenseType.TrainDriver, 
    ];
    public static int GetOrderFromGeneralLicense(GeneralLicenseType_v2 generalLicense) => Array.IndexOf(GeneralLocationsOrder, generalLicense.v1);
    public static long GetIdFromGeneralLicense(GeneralLicenseType_v2 generalLicense) {
        int order = GetOrderFromGeneralLicense(generalLicense);
        return order < 0 ? -1L : order + LOC_GENERAL_LICENSES;
    }
    public static GeneralLicenseType_v2 GetGeneralLicenseFromId(long id) =>
        GeneralLocationsOrder.ElementAtOrDefault(id.Offset(LOC_GENERAL_LICENSES)).ToV2();
    
    #endregion
    #region Mappings long id Items -> DV ITems/Events
    
    public const long ITEMS = 0x100;
    public const long SLICENSES = 0x200;
    public const long GLICENSES = 0x300;
    public const long JLICENSES = 0x310;
    public const long RELIC = 0x350;
    public const long GARAGES = 0x360;
    
    public static ArchipelagoItem GetAPItem(int idx, ItemInfo item) {
        return item.ItemId switch {
            -1 => new AP_Nothing(idx, item),
            1 => new AP_Money(idx, item),
            2 => new AP_DoubleToken(idx, item),
            >= 0x100 and < 0x200 => new AP_PhysicalItem(idx, item),
            >= 0x200 and < 0x300 => new AP_StationLicense(idx, item),
            >= 0x300 and < 0x310 => new AP_GeneralLicense(idx, item),
            >= 0x310 and < 0x320 => new AP_JobLicense(idx, item),
            >= 0x350 and < 0x360 => new AP_RelicLoco(idx, item),
            >= 0x360 and < 0x370 => new AP_CrewVehicle(idx, item),
            _ => throw new ArgumentException($"Invalid item id: {item.ItemId}")
        };
    }
    
    public static string GetStationNameFromId(long id) => StationOrder.ElementAtOrDefault(id.Offset(SLICENSES));
    
    public static TrainCarType GetCarTypeFromId(long id) => TrainTypeOrder.ElementAtOrDefault(id.Offset(RELIC));
    public static int GetOrderFromRelicId(long id) => id.Offset(RELIC);
    
    private static readonly GeneralLicenseType[][] IdToGeneralLicense = [
        [GeneralLicenseType.Dispatcher1], 
        [GeneralLicenseType.TrainDriver], 
        [GeneralLicenseType.DE2], 
        [GeneralLicenseType.DM3], 
        [GeneralLicenseType.DH4], 
        [GeneralLicenseType.DE6], 
        [GeneralLicenseType.S060], 
        [GeneralLicenseType.SH282], 
        [GeneralLicenseType.MultipleUnit], 
        [GeneralLicenseType.MuseumCitySouth], 
        [GeneralLicenseType.ManualService], 
        [GeneralLicenseType.ConcurrentJobs1, GeneralLicenseType.ConcurrentJobs2]
    ];
    public static GeneralLicenseType_v2[] GetGeneralLicenseFamilyFromId(long id) {
        try {
            return [.. IdToGeneralLicense[id-GLICENSES].Select(l => l.ToV2())];
        } catch (IndexOutOfRangeException) {
            return [];
        }
    }
    public static long GetIdFromGarage(GarageType_v2 garage) {
        int order = GetOrderFromGarage(garage);
        return order < 0 ? -1L : LOC_GARAGE_UNLOCKED + order;
    }
    private static readonly JobLicenses[][] IdToJobLicense = [
        [JobLicenses.FreightHaul], 
        [JobLicenses.LogisticalHaul], 
        [JobLicenses.Shunting], 
        [JobLicenses.Fragile], 
        [JobLicenses.TrainLength1, JobLicenses.TrainLength2], 
        [JobLicenses.Hazmat1, JobLicenses.Hazmat2, JobLicenses.Hazmat3], 
        [JobLicenses.Military1, JobLicenses.Military2, JobLicenses.Military3]
    ];
    public static JobLicenseType_v2[] GetJobLicenseFamilyFromId(long id) {
        try {
            return [.. IdToJobLicense[id.Offset(JLICENSES)].Select(l => l.ToV2())];
        } catch (IndexOutOfRangeException) {
            return [];
        }
    }
    
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
    public static string GetItemPrefabFromId(long id) => AddressToItemName[id - ITEMS];
    
    public static LocoRestorationController GetLocoControllerFromId(long id) =>
        LocoRestorationController.GetForLivery(GetCarTypeFromId(id).ToV2());
    
    public static void AcquireStationLicense(string name) {
        GameObject license = BookletCreator_CashRegisterReceipt.Create(GetStationLicenseData(name), Main.Player.Position, Main.Player.Rotation, WorldMover.OriginShiftParent);
        license.name=name+"SL";
        InventoryItemSpec item = license.GetComponent<InventoryItemSpec>();
        item.BelongsToPlayer = true;
        item.name=name+" station license";
        ItemBase component = item.GetComponent<ItemBase>();
        SingletonBehaviour<StorageController>.Instance.AddItemToWorldStorage(component);
    }
    
    public static GarageType_v2 GetGarageFromId(long id) {
        return id switch {
            0x360 or 0x692 => Garage.Bob.ToV2(),
            0x361 or 0x691=> Garage.Caboose.ToV2(),
            0x362 or 0x690=> Garage.DE6_Slug.ToV2(),
            0x363 or 0x693=> Garage.DM1U.ToV2(),
            _ => throw new ArgumentException("GetGarageFromId: Id is not a Garage")
        };
    }

    public static int GetOrderFromGarage(GarageType_v2 garage) =>
        garage.v1 switch {
            Garage.Bob => 0,
            Garage.Caboose => 1,
            Garage.DE6_Slug => 2,
            Garage.DM1U => 3,
            _ => -1
        };
    
    #endregion
    #region Display names utilities

    public static string GetFromFlags(ItemFlags flags) =>
        flags.HasFlag(ItemFlags.Advancement) ? "!!" :
        flags.HasFlag(ItemFlags.NeverExclude) ? "!" :
        flags.HasFlag(ItemFlags.Trap) ? "..." : "";

    
    public static string GetLocoNameFromType(TrainCarType carType) =>
        carType switch {
            TrainCarType.LocoDH4 => "DH4",
            TrainCarType.LocoSteamHeavy => "S282",
            TrainCarType.LocoS060 => "S060",
            TrainCarType.LocoDiesel => "DE6",
            TrainCarType.LocoDM3 => "DM3",
            TrainCarType.LocoShunter => "DE2",
            _ => "Unknown locomotive"
        };
    public static string GetNameFromGarageID(long id) =>
        id switch {
            0x360 => "BE2",
            0x361 => "Caboose",
            0x362 => "DE6 Slug",
            0x363 => "DM1U",
            _ => throw new ArgumentException("Asked for garage name but is not a garage ID")
        };
    
    public static string GetRelicNameFromId(long id) =>
        GetCarTypeFromId(id) switch {
            TrainCarType.LocoShunter => "DE2",
            TrainCarType.LocoSteamHeavy => "S282",
            TrainCarType.LocoS060 => "S060",
            TrainCarType.LocoDiesel => "DE6",
            TrainCarType.LocoDM3 => "DM3",
            TrainCarType.LocoDH4 => "DH4",
            _ => "ERROR"
        };
    public static Sprite GetStationSprite(string name) {
        if (name.Equals("HMB")) name = "HB";
        else if (name.Equals("MFMB")) name = "MF";
        Texture2D icon = new(4,4);
        ImageConversion.LoadImage(icon, File.ReadAllBytes(Path.Combine(Main.Mod!.Path,"icons", $"icon_{name}.png")));
        return Sprite.Create(icon, new Rect(0,0,256,256), new Vector2(0.5f, 0.5f));
    }
    private static CashRegisterModule.CashRegisterModuleData GetFreightData(string name, int idx) {
        return new CashRegisterModule.CashRegisterModuleData {
            unitsToBuy=idx+1,
            pricePerUnit=0,
            resourceName="Freight n°"+(idx+1)+": "+Main.Player.GetItemNameFromLocationId(0x4000+GetOrderFromStationName(name)*0x100+idx, true),
            resourceIcon= TrainCarType.LocoDiesel.ToV2().icon,
            car=null
        };
    }
    private static CashRegisterModule.CashRegisterModuleData GetShuntingData(string name, int idx) {
        return new CashRegisterModule.CashRegisterModuleData {
            unitsToBuy=idx+1,
            pricePerUnit=0,
            resourceName="Shunting n°"+(idx+1)+": "+Main.Player.GetItemNameFromLocationId(0x2000+GetOrderFromStationName(name)*0x100+idx, true),
            resourceIcon= TrainCarType.LocoShunter.ToV2().icon,
            car=null
        };
    }
    public static List<CashRegisterModule.CashRegisterModuleData> GetStationLicenseData(string name) {
        List<CashRegisterModule.CashRegisterModuleData> stationLicense = [new() {
            unitsToBuy=1,
            pricePerUnit=0,
            resourceName=name+" station license",
            resourceIcon=GetStationSprite(name),
            car=null
        }];
        if (!Main.Player.Config.HintsOnStationLicense) return stationLicense;
        int order = GetOrderFromStationName(name);
        for (int i = 0; i < Main.Player.Config.FreightThreshold[order]; i++)
            stationLicense.Add(GetFreightData(name, i));
        for (int i = 0; i < Main.Player.Config.ShuntThreshold[order]; i++)
            stationLicense.Add(GetShuntingData(name, i));
        return stationLicense;
            
    }
    public static string GetStationNameFromFinishingJobId(long id) => GetStationNameFromOrder((int)(id & 0x1F00)>>8);
    #endregion
}