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
    
public class RandoPlayer
{
    internal class DemoLocoListener {
        private readonly long _checkId;
        private readonly Vector3 _locoPosition;
        private float _lastTime;
        private readonly float _spatialThreshold;
        private readonly float _timeThreshold;
        private readonly int _idx;

        public DemoLocoListener(int idx, float spatialThreshold = 5f, float timeThreshold = 20f) {
            _spatialThreshold = spatialThreshold;
            _timeThreshold = timeThreshold;
            (_locoPosition, _checkId) = RandoCommonData.GetInfoRestorationFromLocoLocationOrder(idx);
            _lastTime = 0f;
            _idx = idx;
        }
        public void CheckPosition() {
            if (PlayerManager.PlayerTransform == null) return;
            if (!(Time.time - _lastTime > _timeThreshold) ||
                !((PlayerManager.PlayerTransform.AbsolutePosition() - _locoPosition).magnitude <
                  _spatialThreshold)) return;
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

    public Vector3 Position => PlayerManager.ActiveCamera.transform.position + PlayerManager.ActiveCamera.transform.forward * 0.5f;
    public Quaternion Rotation => PlayerManager.ActiveCamera.transform.rotation;
    public RandoSaveData Data {get;}
    public DVConfig Config => Data.Config;
    private readonly ConcurrentQueue<DV_APItem> _waitingQueue = new();
    private static PauseMenu Menu => UnityEngine.Object.FindObjectOfType<PauseMenu>();
    public ArchipelagoSession Session;
    public APSlotData SlotData {get;}
    public event Action UpdateEvent;
    public DeathLinkService deathLinkService ;

    private void KeyBindControl() {
        if (Input.GetKeyDown("[0]")) {
            Input.ResetInputAxes();
            LocoRestorationControllerPatch.IsIgnoring = true;
            CarsSaveManager.DeleteAllExistingCars();
            for (int i = 0; i < 6; i++) {
                if (Data.ReceivedRelics[i] == 0) continue;
                TrainCarLivery carLivery = RandoCommonData.GetCarTypeFromOrder(i).ToV2();
                LocoRestorationController controller = LocoRestorationController.GetForLivery(carLivery);
                
                TrainCar[] carSpawned = AP_RelicLoco.SpawnRelic(
                    controller.garageSpawner.locoSpawnPoint.transform.position,
                    controller.locoLivery, controller.garageSpawner.flipSpawnLoco);
                foreach (TrainCar car in carSpawned) AP_RelicLoco.SetupRelic(car);
                controller.loco = carSpawned[0];
                controller.saveData.SetString("loco", controller.loco.CarGUID);

                if (controller.secondCarLivery != null) {
                    controller.secondCar = carSpawned[1];
                    controller.saveData.SetString("secondCar", controller.secondCar!.CarGUID);
                }

                controller.SetState(LocoRestorationController.RestorationState.S4_OnDestinationTrack);

            }
            LocoRestorationControllerPatch.IsIgnoring = false;
        }
        else if (Input.GetKeyDown("[1]"))
        {
            Input.ResetInputAxes();
            LocoRestorationController.GetForLivery(TrainCarType.LocoDM3.ToV2()).OnPartsOrdered();
        }
        
    }

    public bool AddLocation(long id) {
        return Data.LocationsChecked.Add(id);
    }
    public void InitGame() {
        //Check if we need to resync (items received while we were offline)
        int itemNumberReceived = Session.Items.AllItemsReceived.Count;
        if (Data.Index < itemNumberReceived) {
            Main.Log($"Re-syncing...");
            for (int id = Data.Index ; id < itemNumberReceived; id++) {
                DV_APItem item = RandoCommonData.GetAPItem(id, Session.Items.AllItemsReceived[id]);
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
        UpdateEvent += KeyBindControl;
    }
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
    public void CallUpdate() {
        UpdateEvent?.Invoke();
    }
    #endregion
    #region Network methods helpers
    public ItemInfo UnlockCheck(long checkId) {
        Session.Locations.CompleteLocationChecks(checkId);
        var askTask = Session.Locations.ScoutLocationsAsync(checkId);
        askTask.Wait();
        return askTask.Result[checkId];
    }
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
    private void ProcessItems() {
        if (_waitingQueue.TryDequeue(out DV_APItem item)){
            item.Acquire().Wait();
        }
    }
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

    public void ReceivedError(Exception e, string message) {
        //Terminal.Log(TerminalLogType.Error, "[AP] Error "+e+":"+message);
        Main.Error("[AP] "+message);
    }
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
    public string GetItemNameFromLocationId(long id, bool asHint=false) {
        Task<Dictionary<long, ScoutedItemInfo>> ask = Session.Locations.ScoutLocationsAsync(asHint?HintCreationPolicy.CreateAndAnnounceOnce:HintCreationPolicy.None, id);
        ask.Wait();
        ScoutedItemInfo info = ask.Result[id];
        return info.ItemDisplayName+" ("+info.Player.Name+")";
    }
        
    #endregion
    #region Acquiring items
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
    public void AddToken() => Data.Tokens++;
    public int AddRelic(long id) =>
        ++Data.ReceivedRelics[RandoCommonData.GetOrderFromRelicId(id)];
    
    public void AcquireLicense(string station) {
        Data.StationLicenses[RandoCommonData.GetOrderFromStationName(station)] = true;
    }
    public (int, ItemInfo) FinishLoco(TrainCar car) {
        if (car == null) return (-1, null);
        int locoIdx = RandoCommonData.GetOrderFromLocoType(car.carType);
        int remaining = Data.Config.LocoJobsThreshold[locoIdx] - ++Data.LocoJobs[locoIdx];
        ItemInfo item = remaining == 0 ? UnlockCheck(RandoCommonData.GetIdLocoJobsFromOrder(locoIdx)) : null;
        return (Math.Max(0, remaining), item);
    }
    public (int, int) GetShuntingData(string station) {
        int stIdx = RandoCommonData.GetOrderFromStationName(station);
        return (Data.Shunts[stIdx], Data.Config.ShuntThreshold[stIdx]);
    }
    public (int, int) GetTransportData(string station) {
        int stIdx = RandoCommonData.GetOrderFromStationName(station);
        return (Data.Freights[stIdx], Data.Config.FreightThreshold[stIdx]);
    }
    public (int, int) GetVictoryData(string station) {
        int stIdx = RandoCommonData.GetOrderFromStationName(station);
        return (Data.Freights[stIdx]+Data.Shunts[stIdx], Data.Config.VictoryThreshold);
    }
    public (int, ItemInfo) FinishShunting(string station) {
        int stOrder = RandoCommonData.GetOrderFromStationName(station);
        long checkId = RandoCommonData.ComputeCheckForJob(true, station, Data.Shunts[stOrder]);
        Data.Shunts[stOrder] += 1;
        int remaining = Data.Config.ShuntThreshold[stOrder] - Data.Shunts[stOrder];
        return remaining >= 0 ? (remaining, UnlockCheck(checkId)) : (0, null);
    }
    public (int, ItemInfo) FinishTransport(string station) {
        int stOrder = RandoCommonData.GetOrderFromStationName(station);
        long checkId = RandoCommonData.ComputeCheckForJob(false, station, Data.Freights[stOrder]);
        Data.Freights[stOrder] += 1;
        int remaining = Data.Config.FreightThreshold[stOrder] - Data.Freights[stOrder];
        return remaining >= 0 ? (remaining, UnlockCheck(checkId)) : (0, null);
    }

    #endregion
    #region Checking player possibilities

    public bool HasChecked(JobLicenseType_v2 jobLicense) =>
        Data.JobLocations[RandoCommonData.GetOrderFromJobLicense(jobLicense)];
    
    public bool HasChecked(GeneralLicenseType_v2 generalLicense) =>
        Data.GeneralLocations[RandoCommonData.GetOrderFromGeneralLicense(generalLicense)];
    

    public bool GotStationLicense(string name) {
        return Data.StationLicenses[RandoCommonData.GetOrderFromStationName(name)];
    }

    public bool CanFinishRelic(long id) {
        return Data.ReceivedRelics[RandoCommonData.GetOrderFromRelicId(id)] > 1;
    }
    public bool CanFinishRelic(TrainCarType carType) {
        return Data.ReceivedRelics[RandoCommonData.GetOrderFromLocoType(carType)] == 2;
    }
    
    public bool HasUnlocked(GarageType_v2 g) =>
        g.v1 switch
        {
            Garage.Museum_FlatbedShort => SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2()),
            Garage.DE2_Relic or Garage.DM3_Relic or Garage.DH4_Relic or Garage.DE6_Relic or Garage.S060_Relic or Garage.S282_Relic => 
                RandoCommonData.GetState(g.garageCarLiveries[0].v1) >= LocoRestorationController.RestorationState.S9_LocoServiced,
            _ => Data.HiddenGarages.ElementAtOrDefault(RandoCommonData.GetOrderFromGarage(g))
        };
    
        
    public void UnlockGarage(GarageType_v2 garage) =>
        Data.HiddenGarages[RandoCommonData.GetOrderFromGarage(garage)] = true;
    
    public void CheckRestoLoco(int order) =>
        Data.LocoLocations[order] = true;
    
    public void CheckGLicense(GeneralLicenseType_v2 generalLicense) =>
        Data.GeneralLocations[RandoCommonData.GetOrderFromGeneralLicense(generalLicense)] = true;
    
    public void CheckJLicense(JobLicenseType_v2 jobLicense) =>
        Data.JobLocations[RandoCommonData.GetOrderFromJobLicense(jobLicense)] = true;
    

}
#endregion