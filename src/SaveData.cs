using System;
using System.Collections.Generic;

namespace DvMod.Randomizer;

public enum VictoryCond {
    NbOfJobs = 0,
    DemoLocos = 1
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
        LocoLocations = new bool[57],
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