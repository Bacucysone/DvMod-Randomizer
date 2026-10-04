using System;
using System.Collections.Generic;
using System.Linq;

namespace DvMod.Randomizer;

/// <summary>
/// Enumeration of all victory conditions
/// </summary>
public enum VictoryCond {
    NbOfJobs = 0,
    DemoLocos = 1
}

public enum DemoLocoBehaviour {
    Vanilla = 0,
    APGated = 1
}
/// <summary>
/// Class representing the configuration of the game
/// </summary>
[Serializable]
public class DVConfig {
    public int[] ShuntThreshold;
    public int[] FreightThreshold;
    public int[] LocoJobsThreshold;
    public int Victory;
    public int VictoryThreshold;
    public bool HintsOnLocoLicense;
    public bool HintsOnStationLicense;
    public bool HintsOnLicenseManager;
    public bool DeathLink;
    public bool RandomiseLicensePrices;
    public int RandomiseLicensePricesMin;
    public int RandomiseLicensePricesMax;
    public int VictoryDemoLoco;
    public VictoryCond VictoryCondition;
    public DemoLocoBehaviour VanillaDemoLoco;
    public bool RelicSpawnChecks;
    public bool MuseumChecks;
}
/// <summary>
/// Data class containing all elements for the rando-player
/// </summary>
public class RandoSaveData {
    public bool[] StationLicenses;
    public bool[] HiddenGarages;
    public bool[] JobLocations;
    public bool[] GeneralLocations;
    public bool[] LocoLocations;
    public int[] ReceivedRelics;
    public int[] Shunts;
    public int Index;
    public int[] Freights;
    public int[] LocoJobs;
    public bool AlreadyWon;
    public int Version;
    public HashSet<long> LocationsChecked;
    public DVConfig Config;
    public int Tokens;
    public int[] GeneralLicensePrices;
    public int[] JobLicensePrices;
    public int DemoLocosFinished;

    public static RandoSaveData CreateSaveData(DVConfig config) => new() {
        Version = Main.VERSION,
        StationLicenses = new bool[20],
        HiddenGarages = new bool[4],
        JobLocations = new bool[12],
        GeneralLocations = new bool[13],
        LocoLocations = Enumerable.Repeat(!config.RelicSpawnChecks, 75).ToArray(),// If there are no checks on demo loco, treat it as they all have been collected
        ReceivedRelics = new int[6],
        Index = 0,
        Freights = new int[20],
        Shunts = new int[20],
        LocoJobs = new int[6],
        AlreadyWon = false,
        LocationsChecked = [],
        Config = config,
        Tokens = 0,
        GeneralLicensePrices = new int[RandoCommonData.APGeneralLicenses.Length],
        JobLicensePrices = new int[RandoCommonData.APJobLicenses.Length],
        DemoLocosFinished = 0
    };
}