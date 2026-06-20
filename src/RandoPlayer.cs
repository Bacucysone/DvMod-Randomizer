using DV.Utils;
using DV.ThingTypes;
using CommandTerminal;
using DV.LocoRestoration;
using UnityEngine;
using DV.ThingTypes.TransitionHelpers;
using System;
using DV.Booklets;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Enums;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using DV.UI;
using System.Collections;
using DV.OriginShift;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using DV.JObjectExtstensions;
using DvMod.Randomizer.HarmonyPatches;

namespace DvMod.Randomizer;

/// <summary>
/// Data class representing all information needed when finishing a job to unlock checks and notify the player
/// </summary>
public class JobFinishState {
    public bool HasWon;
    public ItemInfo ItemJob1;
    public ItemInfo ItemJob2;
    public ItemInfo ItemLoco;
    public int RemainingForVictory;
    public int RemainingJobs;
    public int RemainingOtherJobs;
    public int RemainingLoco;
    public bool GotStationLicense;
    public bool IsShunting;
    public string Station;
    public TrainCarType? LastCar;
    public int Tokens;
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
    public bool DeathLink;
}
/// <summary>
/// Data class containing all elements for the randoplayer
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
        Tokens = 0
    };
}
/// <summary>
/// Main class representing the rando player:
/// contains the multiclient.net Session (to connect and exchange with archipelago server)
/// and all progress pertaining to randomizer (tracking of locations sent and items received)
/// </summary>
public class RandoPlayer
{
    /// <summary>
    /// Helper class to scan the demonstrator possible locations to check the corresponding positions
    /// </summary>
    internal class DemoLocoListener {
        
        /// <summary>
        /// Archipelago location ID for current listener
        /// </summary>
        private readonly long _checkId;
        
        /// <summary>
        /// World position of the AP location
        /// </summary>
        private readonly Vector3 _locoPosition;
        
        /// <summary>
        /// Time of last check
        /// </summary>
        private float _lastTime;
        
        /// <summary>
        /// How close you need to be to trigger the check
        /// </summary>
        private readonly float _spatialThreshold;
        
        /// <summary>
        /// How often the check is triggered. Allow for fewer computations by not checking and sending notifications
        /// every frame
        /// </summary>
        private readonly float _timeThreshold;
        
        /// <summary>
        /// Number of the location check
        /// </summary>
        private readonly int _idx;

        public DemoLocoListener(int idx, float spatialThreshold = 5f, float timeThreshold = 20f) {
            _spatialThreshold = spatialThreshold;
            _timeThreshold = timeThreshold;
            (_locoPosition, _checkId) = RandoCommonData.GetInfoRestorationFromLocoLocationOrder(idx);
            _lastTime = 0f;
            _idx = idx;
        }
        
        /// <summary>
        /// Actual routine of position checking.
        /// Check every frame if the conditions are met (player close enough and last check long enough)
        /// </summary>
        public void CheckPosition() {
            if (PlayerManager.PlayerTransform == null) return;
            if (Time.time - _lastTime <= _timeThreshold ||
                (PlayerManager.PlayerTransform.AbsolutePosition() - _locoPosition).magnitude >=
                  _spatialThreshold) return;
            string stationNeeded = RandoCommonData.GetStationFromLocoLocations(_locoPosition);
            bool stationOk = Main.Player.GotStationLicense(stationNeeded);
            bool museumOk = SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2());
            if (stationOk && museumOk) {
                ItemInfo item = Main.Player.UnlockCheck(_checkId);
                Main.Player.CheckRestoLoco(_idx);
                Main.NotifyPlayer($"You found a {item.ItemDisplayName} for {item.Player.Name} on the ground!");
                Main.Player.UpdateEvent -= CheckPosition;
            } else {
                _lastTime = Time.time;
                if (stationOk)
                    Main.NotifyPlayer("There is something here but you cannot take it... You need the museum license");
                else if (museumOk)
                    Main.NotifyPlayer("There is something here but you cannot take it... You need the "+stationNeeded+" station license");
                else
                    Main.NotifyPlayer("There is something here but you cannot take it... You need the museum license and the "+stationNeeded+" station license");
            }
        }
    }
    #region Player fields, properties and constructor/destructor

    /// <summary>
    /// Shortcut to the player position
    /// </summary>
    public Vector3 Position => PlayerManager.ActiveCamera.transform.position + PlayerManager.ActiveCamera.transform.forward * 0.5f;
    /// <summary>
    /// Shortcut to the player rotation
    /// </summary>
    public Quaternion Rotation => PlayerManager.ActiveCamera.transform.rotation;
    /// <summary>
    /// All information needed to be stored by the rando-player as a data class
    /// </summary>
    public RandoSaveData Data {get;}
    /// <summary>
    /// Game configuration as sent by the AP server
    /// </summary>
    public DVConfig Config => Data.Config;
    /// <summary>
    /// Queue of all AP items received but not yet processed
    /// </summary>
    private readonly ConcurrentQueue<ArchipelagoItem> _waitingQueue = new();
    /// <summary>
    /// PauseMenu object of Derail Valley, used to access the events fired when the user quits the game
    /// or goes back to the main menu
    /// </summary>
    private static PauseMenu Menu => UnityEngine.Object.FindObjectOfType<PauseMenu>();
    /// <summary>
    /// Session provided by multiclient.net, allows for all communication with the AP server
    /// </summary>
    public ArchipelagoSession Session;
    /// <summary>
    /// Slot data sent when connected, contains the configuration of the game
    /// </summary>
    public APSlotData SlotData {get;}
    /// <summary>
    /// Event fired every frame, any routine that must run regularly can subscribe
    /// </summary>
    public event Action UpdateEvent;
    /// <summary>
    /// Provided by multiclient.net, helper for the deathlink option
    /// </summary>
    public DeathLinkService deathLinkService ;
    
    /// <summary>
    /// Register the location checked
    /// </summary>
    /// <param name="id">The Archipelago location id</param>
    /// <returns>true iff location of id <paramref name="id"/> was not checked before</returns>
    public bool AddLocation(long id) {
        return Data.LocationsChecked.Add(id);
    }
    
    /// <summary>
    /// Everything that needs to be done when the rando-player is loaded (so cannot happen in the constructor), but
    /// before the game finished loading
    /// </summary>
    public void InitGame() {
        //Check if we need to resync (items received while we were offline)
        int itemNumberReceived = Session.Items.AllItemsReceived.Count;
        if (Data.Index < itemNumberReceived) {
            Main.Log($"Re-syncing...");
            for (int id = Data.Index ; id < itemNumberReceived; id++) {
                ArchipelagoItem item = RandoCommonData.GetAPItem(id, Session.Items.AllItemsReceived[id]);
                _waitingQueue.Enqueue(item);
            }
            Data.Index = itemNumberReceived;
        }
        SetupListeners(true);
        UpdateEvent += ProcessItems;
        //Add prices for normally tutorial acquired licenses
        GeneralLicenseType.DE2.ToV2().price = 5000;
        GeneralLicenseType.TrainDriver.ToV2().price = 1000;
        JobLicenses.FreightHaul.ToV2().price = 10000;
        TrainCarType.LocoShunter.ToV2().requiredLicense = GeneralLicenseType.DE2.ToV2();
        //Set up demo loco locations
        for (int i = 0; i < Data.LocoLocations.Count(); i++) {
            if (!Data.LocoLocations[i])
                UpdateEvent += new DemoLocoListener(i).CheckPosition;
        }
    }
    /// <summary>
    /// Coroutine to disconnect the player when the game is unloaded (to main menu)
    /// </summary>
    private IEnumerator Subscribe() {
        while (Menu == null) yield return null;
        Menu.controller.ExitLevelRequested += Main.Disconnect;
        Menu.controller.QuitGameRequested += Main.Disconnect;
    }
    
    public RandoPlayer(RandoSaveData saveData) {
        (string server, string password, string slotName, int port) = 
            (Main.Settings!.serverName, Main.Settings.Password, Main.Settings.User, Main.Settings.Port);
        Session = ArchipelagoSessionFactory.CreateSession(server, port);
        LoginResult login = Session.TryConnectAndLogin("Derail Valley", slotName, ItemsHandlingFlags.AllItems, password: password);
        if (login is LoginFailure failLogin) {
            Main.Log("Error! We got the following error while connecting: "+failLogin.Errors.Aggregate((acc, s) => acc+"/"+s));
            Main.NotifyPlayer("Archipelago server connection failed. Please check that the server is up and running and that you provided the correct connection information."); 
            MainMenu.GoBackToMainMenu();
            throw new Exception();
        }
        SlotData = ((LoginSuccessful)login).SlotData;
        SingletonBehaviour<CoroutineManager>.Instance.Run(Subscribe());
        Data = saveData ?? RandoSaveData.CreateSaveData(SlotData.Config);
        if (!Data.Config.DeathLink) return;
        deathLinkService = Session.CreateDeathLinkService();
        deathLinkService.OnDeathLinkReceived += DeathLinkPatch.Derail;
        deathLinkService.EnableDeathLink();
    }
    
    /// <summary>
    /// Destructor of the player (disconnect the websocket, remove any impact of the player on other parts of the game,
    /// make sure that unprocessed items are not lost when reloading)
    /// </summary>
    public void Dispose() {
        if (Menu != null && Menu.controller != null) {
            Menu.controller.ExitLevelRequested -= Main.Disconnect;
            Menu.controller.QuitGameRequested -= Main.Disconnect;
        }
        Data.Index -= _waitingQueue.Count;
        SetupListeners(false);
        deathLinkService = null;
        Session.Socket.DisconnectAsync();
        UpdateEvent = null;
    }
    /// <summary>
    /// Fires the update event, should only be called by a single other class in charge of running once every frame
    /// </summary>
    public void CallUpdate() => UpdateEvent?.Invoke();
    
    #endregion
    #region Network methods helpers
    /// <summary>
    /// Sens to AP server that a location has been checked
    /// </summary>
    /// <param name="checkId">The archipelago location id</param>
    /// <returns>The item that was sent, for display purposes</returns>
    public ItemInfo UnlockCheck(long checkId) {
        Session.Locations.CompleteLocationChecks(checkId);
        var askTask = Session.Locations.ScoutLocationsAsync(checkId);
        askTask.Wait();
        return askTask.Result[checkId];
    }
    
    /// <summary>
    /// Helper function to subscribe to the different events of the AP session
    /// </summary>
    /// <param name="on">if true, subscribe, else, unsubscribe</param>
    private void SetupListeners(bool on) {
        if (on) {
            Session.Items.ItemReceived += ReceivedItem;
            Session.MessageLog.OnMessageReceived += ReceivedMessage;
            Session.Socket.ErrorReceived += ReceivedError;
        } else {
            Session.Items.ItemReceived -= ReceivedItem;
            Session.MessageLog.OnMessageReceived -= ReceivedMessage;
            Session.Socket.ErrorReceived -= ReceivedError;
        }
    }
    /// <summary>
    /// Function that runs every frame and process the received items.
    /// Bugs were reported (and observed) when the items were processed on the same thread as the session communications,
    /// by moving the item processing loop on the UpdateEvent, we move to another thread and solving these problems
    /// </summary>
    private void ProcessItems() {
        if (_waitingQueue.TryDequeue(out ArchipelagoItem item)){
            item.Acquire().Wait();
        }
    }
    /// <summary>
    /// Called whenever the user receive an item. It enqueues the item in <see cref="_waitingQueue"/> and check that
    /// we did not miss any items by re-syncing if necessary
    /// </summary>
    /// <param name="itemHelper">Helper provided by multiclient.net for received items</param>
    private void ReceivedItem(ReceivedItemsHelper itemHelper) {
        Queue<ItemInfo> currQueue = new();
        while (itemHelper.Any()) {
            currQueue.Enqueue(itemHelper.DequeueItem());
        }
        if (itemHelper.Index == Data.Index + currQueue.Count) {
            while (currQueue.Any()) {
                _waitingQueue.Enqueue(RandoCommonData.GetAPItem(Data.Index++, currQueue.Dequeue()));
            }
        } else {
            while (Data.Index < itemHelper.Index)
                _waitingQueue.Enqueue(RandoCommonData.GetAPItem(Data.Index, itemHelper.AllItemsReceived[Data.Index++]));
        }
    }
    /// <summary>
    /// Process and print errors received by AP server
    /// </summary>
    /// <param name="e">The exception thrown</param>
    /// <param name="message">Additional message if applicable</param>
    public void ReceivedError(Exception e, string message) {
        //Terminal.Log(TerminalLogType.Error, "[AP] Error "+e+":"+message);
        Main.Error("[AP] "+message);
    }
    /// <summary>
    /// Process and print messages received by AP server
    /// </summary>
    /// <param name="message">The message received</param>
    public void ReceivedMessage(LogMessage message) {
        switch (message) {
            case AdminCommandResultLogMessage:
                Terminal.Log(TerminalLogType.Input, "[ADMIN] "+message);
                break;
            case ServerChatLogMessage:
                Terminal.Log(TerminalLogType.Message, message.ToString());
                break;
            case ItemSendLogMessage:
                Terminal.Log(TerminalLogType.Warning, message.ToString());
                break;
            case CommandResultLogMessage:
            case TutorialLogMessage:
            case CountdownLogMessage:
                Terminal.Log(TerminalLogType.Input, message.ToString());
                break;
            case ChatLogMessage chat:
                if (!chat.IsActivePlayer)
                    Terminal.Log(TerminalLogType.Message, chat.ToString());
                break;
        }
    }
    /// <summary>
    /// Ask the server for the item name of a specific location in the current Derail Valley client
    /// </summary>
    /// <param name="id">The Archipelago location id</param>
    /// <param name="asHint">If true, tell the server to create a hint for this scout. Useful for external trackers</param>
    /// <returns>The name of the item, with the slot name of the receiving player</returns>
    public string GetItemNameFromLocationId(long id, bool asHint=false) {
        Task<Dictionary<long, ScoutedItemInfo>> ask = Session.Locations.ScoutLocationsAsync(asHint?HintCreationPolicy.CreateAndAnnounceOnce:HintCreationPolicy.None, id);
        ask.Wait();
        ScoutedItemInfo info = ask.Result[id];
        return info.ItemDisplayName+" ("+info.Player.Name+")";
    }
        
    #endregion
    #region Acquiring items
    /// <summary>
    /// Change the internal state of the rando-player and notify the server of the corresponding locations when finishing a job
    /// </summary>
    /// <param name="data">The information of the job that was finished</param>
    /// <returns>A JobFinishState data that contains the items unlocked by the job, diverse information on remaining locations
    /// and progression towards victory</returns>
    public JobFinishState FinishJob(Job_data data) {
        string station = data.type switch {
            JobType.ShuntingUnload => data.chainDestinationStationInfo.YardID,
            _ => data.chainOriginStationInfo.YardID
        };
        bool isShunting = data.type == JobType.ShuntingLoad || data.type == JobType.ShuntingUnload;
        int stOrder = RandoCommonData.GetOrderFromStationName(station);
        if (!GotStationLicense(station)) {
            return new() {
                HasWon = Data.AlreadyWon,
                ItemJob1 = null,
                ItemJob2 = null,
                ItemLoco = null,
                RemainingForVictory = Main.Player.Config.VictoryThreshold,
                RemainingLoco = Main.Player.Config.LocoJobsThreshold[0],
                IsShunting = isShunting,
                GotStationLicense = false,
                Station=station,
                RemainingJobs = (isShunting?Main.Player.Config.ShuntThreshold:Main.Player.Config.FreightThreshold)[stOrder],
                RemainingOtherJobs = (!isShunting?Main.Player.Config.ShuntThreshold:Main.Player.Config.FreightThreshold)[stOrder],
                LastCar = null,
                Tokens = Data.Tokens
            };
        }

        (int remaining, ItemInfo item1) = isShunting ? FinishShunting(station) : FinishTransport(station);
        (int otherRem, int otherMax) = isShunting ? GetTransportData(station) : GetShuntingData(station);
        (int remainingLoco, ItemInfo itemLoco) = FinishLoco(PlayerManager.LastLoco);

        ItemInfo item2 = null;
        ItemInfo itemLoco2 = null;
        int remainingForVictory = CheckVictory(station);
        if ((remaining > 0 || remainingLoco > 0 || remainingForVictory > 0) && Data.Tokens > 0) {
            Data.Tokens--;
            (remaining, item2) = isShunting ? FinishShunting(station) : FinishTransport(station);
            (remainingLoco, itemLoco2) = FinishLoco(PlayerManager.LastLoco);
            remainingForVictory = CheckVictory(station);
        }
        return new() {
            HasWon = Data.AlreadyWon,
            ItemJob1 = item1,
            ItemJob2 = item2,
            ItemLoco = itemLoco ?? itemLoco2,
            RemainingForVictory = remainingForVictory,
            IsShunting = isShunting,
            GotStationLicense = true,
            RemainingJobs = remaining,
            RemainingLoco = remainingLoco,
            Station = station,
            RemainingOtherJobs = Math.Max(0, otherMax - otherRem),
            LastCar = PlayerManager.LastLoco?.carType,
            Tokens = Data.Tokens
        };
    }
    /// <summary>
    /// Count the number of finished jobs to check if the game is finished. If so, notify the AP server of victory
    /// </summary>
    /// <param name="station">A station name</param>
    /// <returns>The number of jobs already finished in <paramref name="station"/>, for display purposes</returns>
    public int CheckVictory(string station) {
        int toReturn = -1;
        int stOrder = RandoCommonData.GetOrderFromStationName(station);
        if (Data.AlreadyWon) return toReturn;
        int stationFinished = 0;
        for (int i = 0; i < 20; i++) {
            int currRem = Data.Config.VictoryThreshold - (Data.Shunts[i] + Data.Freights[i]);
            if (currRem <= 0) stationFinished++;
            if (i == stOrder) toReturn = Math.Max(0, currRem);
        }

        if (stationFinished < Data.Config.Victory) return toReturn;
        Terminal.Log(TerminalLogType.Warning, "You won the game!");
        Data.AlreadyWon = true;
        Session.SetGoalAchieved();
        return toReturn;
    }
    /// <summary>
    /// Register when a double job token has been acquired
    /// </summary>
    public void AddToken() => Data.Tokens++;
    
    /// <summary>
    /// Register when a progressive relic loco has been acquired
    /// </summary>
    /// <param name="id">The received Archipelago Item id</param>
    /// <returns>The number of progressive relic loco acquired so far (should only be 1 or 2)</returns>
    public int AddRelic(long id) =>
        ++Data.ReceivedRelics[RandoCommonData.GetOrderFromRelicId(id)];
    
    /// <summary>
    /// Register when a station license has bee acquired
    /// </summary>
    /// <param name="station">The station name of the acquired license</param>
    public void AcquireLicense(string station) =>
        Data.StationLicenses[RandoCommonData.GetOrderFromStationName(station)] = true;
    
    /// <summary>
    /// Change internal state when finishing a job with a specific loco. If applicable, notify the AP server of a new location
    /// check
    /// </summary>
    /// <param name="car">The last loco of the player</param>
    /// <returns>A tuple that contains <list type="bullet">
    /// <item><description>The number of jobs that still need to be finished to get an item,
    /// or -1 if the car is not a valid locomotive</description> </item>
    /// <item><description>The item that was unlocked, or null if no items were unlocked</description></item>
    /// </list></returns>
    public (int, ItemInfo) FinishLoco(TrainCar car) {
        if (car == null) return (-1, null);
        int locoIdx = RandoCommonData.GetOrderFromLocoType(car.carType);
        int remaining = Data.Config.LocoJobsThreshold[locoIdx] - ++Data.LocoJobs[locoIdx];
        ItemInfo item = remaining == 0 ? UnlockCheck(RandoCommonData.GetIdLocoJobsFromOrder(locoIdx)) : null;
        return (Math.Max(0, remaining), item);
    }
    /// <summary>
    /// Get the information of shunting jobs done in <paramref name="station"/>
    /// </summary>
    /// <param name="station">The requested station name</param>
    /// <returns>A tuple (Nb of shunting jobs already finished, Nb of shunting jobs that award items)</returns>
    public (int, int) GetShuntingData(string station) {
        int stIdx = RandoCommonData.GetOrderFromStationName(station);
        return (Data.Shunts[stIdx], Data.Config.ShuntThreshold[stIdx]);
    }
    /// <summary>
    /// Get the information of transport jobs done in <paramref name="station"/>
    /// </summary>
    /// <param name="station">The requested station name</param>
    /// <returns>A tuple (Nb of transport jobs already finished, Nb of transport jobs that award items)</returns>
    public (int, int) GetTransportData(string station) {
        int stIdx = RandoCommonData.GetOrderFromStationName(station);
        return (Data.Freights[stIdx], Data.Config.FreightThreshold[stIdx]);
    }
    /// <summary>
    /// Get the information of victory progression in <paramref name="station"/>
    /// </summary>
    /// <param name="station">The requested station name</param>
    /// <returns>A tuple (Nb of jobs already finished, Nb of jobs needed to clear the station)</returns>
    public (int, int) GetVictoryData(string station) {
        int stIdx = RandoCommonData.GetOrderFromStationName(station);
        return (Data.Freights[stIdx]+Data.Shunts[stIdx], Data.Config.VictoryThreshold);
    }
    /// <summary>
    /// Change internal state when finishing a shunting job in <paramref name="station"/>. If applicable, notify the AP server of a new location
    /// check
    /// </summary>
    /// <param name="station">The checked station name</param>
    /// <returns>A tuple that contains <list type="bullet">
    /// <item><description>The number of jobs that will still reward items</description> </item>
    /// <item><description>The item that was unlocked, or null if no items were unlocked</description></item>
    /// </list></returns>
    public (int, ItemInfo) FinishShunting(string station) {
        int stOrder = RandoCommonData.GetOrderFromStationName(station);
        long checkId = RandoCommonData.ComputeCheckForJob(true, station, Data.Shunts[stOrder]);
        Data.Shunts[stOrder] += 1;
        int remaining = Data.Config.ShuntThreshold[stOrder] - Data.Shunts[stOrder];
        return remaining >= 0 ? (remaining, UnlockCheck(checkId)) : (0, null);
    }
    /// <summary>
    /// Change internal state when finishing a transport job in <paramref name="station"/> (either logistical or freight).
    /// If applicable, notify the AP server of a new location check
    /// </summary>
    /// <param name="station">The checked station name</param>
    /// <returns>A tuple that contains <list type="bullet">
    /// <item><description>The number of jobs that will still reward items</description> </item>
    /// <item><description>The item that was unlocked, or null if no items were unlocked</description></item>
    /// </list></returns>
    public (int, ItemInfo) FinishTransport(string station) {
        int stOrder = RandoCommonData.GetOrderFromStationName(station);
        long checkId = RandoCommonData.ComputeCheckForJob(false, station, Data.Freights[stOrder]);
        Data.Freights[stOrder] += 1;
        int remaining = Data.Config.FreightThreshold[stOrder] - Data.Freights[stOrder];
        return remaining >= 0 ? (remaining, UnlockCheck(checkId)) : (0, null);
    }

    #endregion
    #region Checking player possibilities
    /// <summary>
    /// Verify if the given fake job license has already been bought
    /// </summary>
    /// <param name="jobLicense">The specified job license</param>
    /// <returns>true iff license has been bought</returns>
    public bool HasChecked(JobLicenseType_v2 jobLicense) =>
        Data.JobLocations[RandoCommonData.GetOrderFromJobLicense(jobLicense)];
    /// <summary>
    /// Verify if the given fake general license has already been bought
    /// </summary>
    /// <param name="generalLicense">The specified general license</param>
    /// <returns>true iff license has been bought</returns>
    public bool HasChecked(GeneralLicenseType_v2 generalLicense) =>
        Data.GeneralLocations[RandoCommonData.GetOrderFromGeneralLicense(generalLicense)];
    
    /// <summary>
    /// Verify if the rando-player has the specified station license
    /// </summary>
    /// <param name="name">The requested station name</param>
    /// <returns>true iff the player has the station license</returns>
    public bool GotStationLicense(string name) =>
        Data.StationLicenses[RandoCommonData.GetOrderFromStationName(name)];

    /// <summary>
    /// Verify if all the progressive relic loco has been received
    /// </summary>
    /// <param name="id">The requested AP item id</param>
    /// <returns>true iff player has received the 2 relic loco</returns>
    public bool CanFinishRelic(long id) =>
        Data.ReceivedRelics[RandoCommonData.GetOrderFromRelicId(id)] > 1;
    /// <summary>
    /// Verify if all the progressive relic loco has been received
    /// </summary>
    /// <param name="carType">The requested car type</param>
    /// <returns>true iff player has received the 2 relic loco</returns>
    public bool CanFinishRelic(TrainCarType carType) =>
        Data.ReceivedRelics[RandoCommonData.GetOrderFromLocoType(carType)] == 2;

    /// <summary>
    /// Verify if the given garage can be spawned (6 demo loco = service done; Museum flatcar = Museum license;
    /// Crew vehicles = spawn rights received through AP)
    /// </summary>
    /// <param name="g">The requested garage</param>
    /// <returns>true iff we can spawn vehicle from garage</returns>
    public bool HasUnlocked(GarageType_v2 g) =>
        g.v1 switch
        {
            Garage.Museum_FlatbedShort => SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2()),
            Garage.DE2_Relic or Garage.DM3_Relic or Garage.DH4_Relic or Garage.DE6_Relic or Garage.S060_Relic or Garage.S282_Relic => 
                RandoCommonData.GetState(g.garageCarLiveries[0].v1) >= LocoRestorationController.RestorationState.S9_LocoServiced,
            _ => Data.HiddenGarages.ElementAtOrDefault(RandoCommonData.GetOrderFromGarage(g))
        };

    
    /// <summary>
    /// Register that a crew vehicle spawn right has been received
    /// </summary>
    /// /// <param name="garage">The garage to unlock</param>
    public void UnlockGarage(GarageType_v2 garage) =>
        Data.HiddenGarages[RandoCommonData.GetOrderFromGarage(garage)] = true;

    /// <summary>
    /// Register that a demo loco position has been checked
    /// </summary>
    /// <param name="order">The number index of the location checked</param>
    public void CheckRestoLoco(int order) =>
        Data.LocoLocations[order] = true;

    /// <summary>
    /// Register that a fake general license has been bought
    /// </summary>
    /// <param name="generalLicense">The bought general license</param>
    public void CheckGLicense(GeneralLicenseType_v2 generalLicense) =>
        Data.GeneralLocations[RandoCommonData.GetOrderFromGeneralLicense(generalLicense)] = true;
    
    /// <summary>
    /// Register that a fake job license has been bought
    /// </summary>
    /// <param name="jobLicense">The bought job license</param>
    public void CheckJLicense(JobLicenseType_v2 jobLicense) =>
        Data.JobLocations[RandoCommonData.GetOrderFromJobLicense(jobLicense)] = true;
}
#endregion