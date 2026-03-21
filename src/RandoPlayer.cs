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
using System.Threading;
using WebSocketSharp;
using System.Linq;
using System.Runtime.InteropServices;
using Archipelago.MultiClient.Net.DataPackage;
using System.Deployment.Internal;
using DV.UI;
using DV.Teleporters;
using System.Collections;
using DV;
using DV.OriginShift;
using DV.Shops;
using Archipelago.MultiClient.Net.Packets;

namespace DvMod.Randomizer
{
    public class RandoSaveData(
        int Version,
        bool[] StationLicenses, 
        bool[] HiddenGarages, 
        bool[] JobLocations,
        bool[] GeneralLocations,
        bool[] LocoLocations,
        int[] ReceivedRelics, 
        int[] Index, 
        int[] Shunts, 
        int[] ShuntThreshold, 
        int[] Freights, 
        int[] FreightThreshold, 
        int[] LocoJobs, 
        int[] LocoJobsThreshold, 
        int Victory, 
        int VictoryThreshold, 
        bool AlreadyWon,
        string TeleportToStation,
        HashSet<long> LocationsChecked) {
        public bool[] StationLicenses = StationLicenses;
        public bool[] HiddenGarages = HiddenGarages;
        public bool[] JobLocations = JobLocations;
        public bool[] GeneralLocations = GeneralLocations;
        public bool[] LocoLocations = LocoLocations;
        public int[] ReceivedRelics = ReceivedRelics;
        public int[] Shunts = Shunts;
        public int[] Index = Index;
        public int[] ShuntThreshold = ShuntThreshold;
        public int[] Freights = Freights;
        public int[] FreightThreshold = FreightThreshold;
        public int[] LocoJobs = LocoJobs;
        public int[] LocoJobsThreshold = LocoJobsThreshold;
        public int Victory=Victory;
        public int VictoryThreshold = VictoryThreshold;
        public bool AlreadyWon = AlreadyWon;
        public int Version = Version;
        public string TeleportToStation = TeleportToStation;
        public HashSet<long> LocationsChecked = LocationsChecked;
    }
    
    public class RandoPlayer
    {
        internal class DemoLocoListener(int idx, float spatialthreshold = 5f, float timeThreshold = 20f) {
            private readonly float SpatialThreshold = spatialthreshold;
            private readonly float TimeThreshold = timeThreshold;
            private Vector3 LocoPosition = RandoCommonData.GetInfoRestorationFromLocoLocationOrder(idx);
            private readonly long CheckId = RandoCommonData.AP_ID.LOC_LOCO_RESTORATION + idx;
            private float LastTime = 0f;
            public void CheckPosition() {
                if (Time.time - LastTime > TimeThreshold && (PlayerManager.PlayerTransform.AbsolutePosition() - LocoPosition).magnitude < SpatialThreshold) {
                    string stationNeeded = RandoCommonData.GetStationFromLocoLocations(LocoPosition);
                    bool StationOk = Main.player!.GotStationLicense(stationNeeded);
                    bool MuseumOk = SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2());
                    if (StationOk && MuseumOk) {
                        ItemInfo item = Main.player.UnlockCheck(CheckId);
                        Terminal.Log(TerminalLogType.Input, $"You found a {item.ItemDisplayName} on the ground!");
                        Main.player.NotifyPlayer($"You found a {item.ItemDisplayName} on the ground!");
                        Main.player.UpdateEvent -= CheckPosition;
                    } else{
                        LastTime = Time.time;
                        if (StationOk && !MuseumOk)
                            Main.player.NotifyPlayer("There is something here but you cannot take it... You need the museum license");
                        else if (!StationOk && MuseumOk)
                            Main.player.NotifyPlayer("There is something here but you cannot take it... You need the "+stationNeeded+" station license");
                        else
                            Main.player.NotifyPlayer("There is something here but you cannot take it... You need the museum license and the "+stationNeeded+" station license");
                    }
                }
            }
        }
    #region Player fields, properties and constructor/destructor

        public Vector3 Position => PlayerManager.ActiveCamera.transform.position + PlayerManager.ActiveCamera.transform.forward * 0.5f;
        public Quaternion Rotation => PlayerManager.ActiveCamera.transform.rotation;
        public RandoSaveData Data {get; private set;}
        public bool IsFirstLoading => !Data.TeleportToStation.IsNullOrEmpty();
        private readonly ConcurrentQueue<DV_APItem> waitingQueue = new();
        public ArchipelagoSession Session;
        public event Action? UpdateEvent;
        public int ItemCount {
            get => Data.Index[0] + Data.Index.Count() -1;
        }
        private void CheckData() {
            
        }
        public void AddLocation(long id) {
            Data.LocationsChecked.Append(id);
        }
        private void InitGame() {
            //Check if the locations match (Can happen if the game crashes and progress is lost)
            foreach (long checkId in Session.Locations.AllLocationsChecked){
                if (!Data.LocationsChecked.Contains(checkId)) {
                    WorldStreamingInit.LoadingFinished += RandoCommonData.GetAPLocation(checkId).EmergencyCheck;
                }
            }
            //Check if we need to resync (items received while we were offline)
            /*int ItemNumberReceived = Session.Items.AllItemsReceived.Count;
            if (ItemCount < ItemNumberReceived) {
                Main.Log($"Re-syncing...");
                for (int id = Data.Index[0]+1 ; id < ItemNumberReceived; id++) {
                    if (!HasAcquired(id)){
                        DV_APItem item = RandoCommonData.GetAPItem(id, Session.Items.AllItemsReceived[id]);
                        Main.Log("Queueing item "+item.DisplayName);
                        waitingQueue.Enqueue(item);
                    }
                }
            }*/
            //Add prices for normally tutorial acquired licenses
            GeneralLicenseType.DE2.ToV2().price = 5000;
            GeneralLicenseType.TrainDriver.ToV2().price = 1000;
            JobLicenses.FreightHaul.ToV2().price = 10000;
            //Set up demo loco locations
            for (int i = 0; i < Data.LocoLocations.Count(); i++) {
                if (!Data.LocoLocations[i])
                    UpdateEvent += new DemoLocoListener(i).CheckPosition;
            }
            if (IsFirstLoading) {
                //All that we have to do the first time
            }
        }
        private void FinishGame() {

        }
        public RandoPlayer(RandoSaveData saveData) {
            Data = saveData;
            CheckData();
            Session = ArchipelagoSessionFactory.CreateSession(Main.settings!.serverName, Main.settings!.Port);
            SetupListeners(true);
            if(!Session.TryConnectAndLogin("Derail Valley", Main.settings!.User, ItemsHandlingFlags.AllItems, password: Main.settings!.Password).Successful)
                throw new TimeoutException();
            SceneSwitcher.SceneRequested += (sc) => {if (sc == DVScenes.MainMenu) Main.player = null;};
            UpdateEvent += ProcessItems;
            InitGame();
            
        }
        ~RandoPlayer() {
            SetupListeners(false);
            Session.Socket.DisconnectAsync();
            FinishGame();
            UpdateEvent = null;
        }
        public void CallUpdate() {
            UpdateEvent?.Invoke();
        }
    #endregion
    #region Network methods helpers
        public ItemInfo UnlockCheck(long checkId) {
            RandoCommonData.GetAPLocation(checkId).FullCheck();
            var askTask = Session.Locations.ScoutLocationsAsync(checkId);
            askTask.Wait();
            return askTask.Result[checkId];
        }
        public bool HasAcquired(int index) {
            return index < Data.Index[0] || Data.Index.Contains(index);
        }
        private void ReNormalize() {
            while (Data.Index.Count() > 1 && Data.Index[0] == Data.Index[1]-1) Data.Index.Skip(1);
        }
        public void AddItem(int index) {
            if (index == -1) return;
            if (index < Data.Index[0]) {
                throw new ArgumentException("Item was already acquired");
            } else if (index == Data.Index[0]+1){
                Data.Index[0] = index;
            } else {
                int smaller = Data.Index.Count(x => x <= index);
                int[] FirstElements = Data.Index.SubArray(0, smaller);
                if (FirstElements.Last() == index) {
                    throw new ArgumentException("Item was already acquired");
                }
                int[] LastElements = Data.Index.SubArray(smaller, Data.Index.Count()-smaller);
                Data.Index = [..FirstElements, index, .. LastElements];
            }
            ReNormalize();
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
        private async void ProcessItems() {
            if (waitingQueue.TryDequeue(out var item)){
                await item.Acquire();
            }
        }
        private void ReceivedItem(ReceivedItemsHelper itemHelper) {
            Queue<ItemInfo> CurrQueue = new();
            while (itemHelper.Any()) {
                ItemInfo item = itemHelper.DequeueItem();
                CurrQueue.Enqueue(item);
            }
            int CurrIdx = itemHelper.Index - CurrQueue.Count() + 1;
            while (CurrQueue.Any()) {
                waitingQueue.Enqueue(RandoCommonData.GetAPItem(CurrIdx++, CurrQueue.Dequeue()));
            }
        }

        public void ReceivedError(Exception e, string message) {
            //Terminal.Log(TerminalLogType.Error, "[AP] Error "+e+":"+message);
            Main.Error("[AP] "+message);
        }
         public void ReceivedMessage(LogMessage message) {
            switch (message) {
                case AdminCommandResultLogMessage:
                Terminal.Log(TerminalLogType.Input, "[ADMIN] "+message.ToString());
                break;
                case ServerChatLogMessage:
                Terminal.Log(TerminalLogType.Message, message.ToString());
                break;
                case ItemSendLogMessage:
                Terminal.Log(TerminalLogType.Warning, message.ToString());
                break;
                case CommandResultLogMessage:
                Terminal.Log(TerminalLogType.Input, message.ToString());
                break;
                case TutorialLogMessage:
                Terminal.Log(TerminalLogType.Input, message.ToString());
                break;
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
            Main.Log($"Asking details for location id {id} and got {ask.Result.Keys.Select(n => n.ToString()).Aggregate((sacc, s) => sacc + ","+s)}");
            ScoutedItemInfo info = ask.Result[id];
            return info.ItemDisplayName+" ("+info.Player.Name+")";
        }
        public void NotifyPlayer(string message) {
            SingletonBehaviour<ACanvasController<CanvasController.ElementType>>.Instance.NotificationManager.ShowNotification(
                message,
                duration: 5f,
                localize: false
            );
        }
        #endregion
        #region Acquiring items
        public void BypassItem(DV_APItem item) => waitingQueue.Enqueue(item);
        public void CheckVictory() {
            if (!Data.AlreadyWon) {
                int StationFinished = 0;
                for (int i = 0; i < 20; i++) {
                    if (Data.Shunts[i] + Data.Freights[i] >= Data.VictoryThreshold) StationFinished++;
                }
                if (StationFinished >= Data.Victory) {
                    Terminal.Log(TerminalLogType.Warning, "You won the game!");
                    Data.AlreadyWon = true;
                    Session.SetGoalAchieved();
                }
            }
        }
        public int AddRelic(long id) {
            return ++Data.ReceivedRelics[id-RandoCommonData.AP_ID.RELIC];
        }
        public void AcquireLicense(string Station) {
            Data.StationLicenses[RandoCommonData.GetOrderFromStationName(Station)] = true;
        }
        public (long, int) FinishLoco(TrainCarType carType) {
            int locoIdx = RandoCommonData.GetOrderFromLocoType(carType);
            if (Data.LocoJobsThreshold[locoIdx] == Data.LocoJobs[locoIdx]) return (-1L, -1);
            Data.LocoJobs[locoIdx]++;
            return (0x600+locoIdx, Data.LocoJobsThreshold[locoIdx] - Data.LocoJobs[locoIdx]);
        }
        public (int, int) GetShuntingData(string station) {
            int StIdx = RandoCommonData.GetOrderFromStationName(station);
            return (Data.Shunts[StIdx], Data.ShuntThreshold[StIdx]);
        }
        public (int, int) GetTransportData(string station) {
            int StIdx = RandoCommonData.GetOrderFromStationName(station);
            return (Data.Freights[StIdx], Data.FreightThreshold[StIdx]);
        }
        public (long, int) FinishShunting(string station) {
            if (station == "HMB")
                station = "HB";
            else if (station == "MFMB")
                station = "MF";
            int StIdx = RandoCommonData.GetOrderFromStationName(station);
            if (Data.Shunts[StIdx] == Data.ShuntThreshold[StIdx])
                return (-1L, -1);
            long checkId = 0x1000+StIdx*0x100+Data.Shunts[StIdx]++;
            CheckVictory();
            return (checkId, Data.ShuntThreshold[StIdx] - Data.Shunts[StIdx]);
        }
        public (long, int) FinishTransport(string station) {
            if (station == "HMB")
                station = "HB";
            else if (station == "MFMB")
                station = "MF";
            int StIdx = RandoCommonData.GetOrderFromStationName(station);
            if (Data.Freights[StIdx] == Data.FreightThreshold[StIdx])
                return (-1L, -1);
            long checkId = 0x2500+StIdx*0x100+Data.Freights[StIdx]++;
            CheckVictory();
            return (checkId, Data.FreightThreshold[StIdx]-Data.Freights[StIdx]);
            
        }

        #endregion
        #region Checking player possibilities
        private bool HasChecked<T>(Func<T, int> f, T value, bool[] map) {
            int id = f(value);
            if (id < 0) return true;
            return map[id];
        }
        public bool HasChecked(Vector3 position) {
            return HasChecked(RandoCommonData.GetIdFromLocoLocations, position, Data.LocoLocations);
        }
        public bool HasChecked(JobLicenseType_v2 jobLicense) {
            return HasChecked(x => RandoCommonData.GetIDFromJobLicense(x).Item2, jobLicense,  Data.JobLocations);
        }
        public bool HasChecked(GeneralLicenseType_v2 generalLicense) {
            return HasChecked(x => RandoCommonData.GetIDFromGeneralLicense(x).Item2, generalLicense, Data.GeneralLocations);
        }

        public bool GotStationLicense(string name) {
            if (name == "HMB")
                return Data.StationLicenses[10]; // Harbour Military Bureau
            else if (name == "MFMB")
                return Data.StationLicenses[14]; // Machine factory Military Bureau
            return Data.StationLicenses[RandoCommonData.GetOrderFromStationName(name)];
        }

        
    

        public bool GotRestorationLoco(long id) {
            return Data.ReceivedRelics[id-RandoCommonData.AP_ID.RELIC] > 0;
        }
        public bool GotRestorationLoco(TrainCarType carType) {
            return Data.ReceivedRelics[RandoCommonData.GetOrderFromLocoType(carType)] > 0;
        }
        public bool CanFinishRelic(long id) {
            return Data.ReceivedRelics[id-RandoCommonData.AP_ID.RELIC] > 1;
        }
        public bool CanFinishRelic(TrainCarType carType) {
            return Data.ReceivedRelics[RandoCommonData.GetOrderFromLocoType(carType)] == 2;
        }
    
        public bool HasUnlocked(GarageType_v2 g) {
            return g.v1 switch
            {
                Garage.Bob => Data.HiddenGarages[0],
                Garage.Caboose => Data.HiddenGarages[1],
                Garage.DE6_Slug => Data.HiddenGarages[2],
                Garage.Museum_FlatbedShort => SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2()),
                Garage.DM1U => Data.HiddenGarages[3],
                Garage.DE2_Relic or Garage.DM3_Relic or Garage.DH4_Relic or Garage.DE6_Relic or Garage.S060_Relic or Garage.S282_Relic => 
                    (RandoCommonData.GetState(g.garageCarLiveries[0].v1) == LocoRestorationController.RestorationState.S9_LocoServiced) 
                 || (RandoCommonData.GetState(g.garageCarLiveries[0].v1) == LocoRestorationController.RestorationState.S10_PaintJobDone),
                _ => false
            };
        }
        
        public bool IsJobLicenseAcquired(JobLicenseType_v2 jobLicense) {
            return Data.JobLocations[RandoCommonData.GetIDFromJobLicense(jobLicense).Item2];
        }
        public bool IsGeneralLicenseAcquired(GeneralLicenseType_v2 jobLicense) {
            return Data.GeneralLocations[RandoCommonData.GetIDFromGeneralLicense(jobLicense).Item2];
        }
        public void UnlockGarage(long id) {
            Data.HiddenGarages[id-RandoCommonData.AP_ID.GARAGES] = true;
        }
        public void CheckRestoLoco(long id) {
            Data.LocoLocations[id - RandoCommonData.AP_ID.LOC_LOCO_RESTORATION] = true;
        }
        public void CheckGLicense(long Id) {
            Data.GeneralLocations[Id - RandoCommonData.AP_ID.LOC_GENERAL_LICENSES] = true;
        }
        public void CheckJLicense(long Id) {
            Data.JobLocations[Id - RandoCommonData.AP_ID.LOC_JOB_LICENSES] = true;
        }

    }
#endregion
}
