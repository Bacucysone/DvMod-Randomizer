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
        string TeleportToStation) {
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
    }

    public class RandoPlayer
    {
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
        public void InitGame() {
            int ItemNumberReceived = Session.Items.AllItemsReceived.Count;
            if (ItemCount < ItemNumberReceived) {
                //We need to resync
                Main.Log($"Re-syncing...");
                for (int id = Data.Index[0]+1 ; id < ItemNumberReceived; id++) {
                    if (!HasAcquired(id)){
                        DV_APItem item = RandoCommonData.GetAPItem(id, Session.Items.AllItemsReceived[id]);
                        Main.Log("Queueing item "+item.DisplayName);
                        waitingQueue.Enqueue(item);
                    }
                }
            }
            //Add prices for usually tutorial acquired licenses
            SetLicenseData(GeneralLicenseType.DE2.ToV2(),5000);
            SetLicenseData(GeneralLicenseType.TrainDriver.ToV2(), 1000);
            SetLicenseData(JobLicenses.FreightHaul.ToV2(), 10000);
            if (IsFirstLoading) {
                //All that we have to do the first time
            }
        }
        private void SetLicenseData(GeneralLicenseType_v2 license, float price) {
            license.price = price;
            license.requiredGeneralLicense = null;
            license.requiredJobLicense = null;
        }
        private void SetLicenseData(JobLicenseType_v2 license, float price) {
            license.price = price;
            license.requiredGeneralLicense = null;
            license.requiredJobLicense = null;
        }
        private void FinishGame() {

        }
        public RandoPlayer(RandoSaveData saveData) {
            Data = saveData;
            CheckData();
            Session = ArchipelagoSessionFactory.CreateSession(Main.settings!.serverName, Main.settings!.Port);
            if(!Session.TryConnectAndLogin("Derail Valley", Main.settings!.User, ItemsHandlingFlags.AllItems, password: Main.settings!.Password).Successful)
                throw new TimeoutException();
            SceneSwitcher.SceneRequested += (sc) => {if (sc == DVScenes.MainMenu) Main.player = null;};
            UpdateEvent += ProcessItems;
            InitGame();
            SetupListeners(true);
        }
        ~RandoPlayer() {
            Session.Socket.DisconnectAsync();
            FinishGame();
            UpdateEvent = null;
            SetupListeners(false);
        }
        internal void CallUpdate() {
            UpdateEvent?.Invoke();
        }
    #endregion
    #region Network methods helpers
        public void UnlockCheck(long checkId) {
            Session.Locations.CompleteLocationChecks(checkId);
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
        private void ProcessItems() {
            if (waitingQueue.TryDequeue(out var item))
                _=item.Acquire();
        }
        private void ReceivedItem(ReceivedItemsHelper itemHelper) {
            if (Main.player == null) return;
            Queue<ItemInfo> CurrQueue = new();
            while (itemHelper.Any()) {
                CurrQueue.Enqueue(itemHelper.DequeueItem());
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
        public void Check(GeneralLicenseType_v2 generalLicense) {
            int id = RandoCommonData.GetIDFromGeneralLicense(generalLicense);
            Data.GeneralLocations[id] = true;
            UnlockCheck(0x660+id);
        }
        public void Check(JobLicenseType_v2 jobLicense) {
            int id = RandoCommonData.GetIDFromJobLicense(jobLicense);
            Data.JobLocations[id] = true;
            UnlockCheck(0x670+id);
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
            return HasChecked(RandoCommonData.GetIDFromJobLicense, jobLicense,  Data.JobLocations);
        }
        public bool HasChecked(GeneralLicenseType_v2 generalLicense) {
            return HasChecked(RandoCommonData.GetIDFromGeneralLicense, generalLicense, Data.GeneralLocations);
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
            return Data.JobLocations[RandoCommonData.GetIDFromJobLicense(jobLicense)];
        }
        public bool IsGeneralLicenseAcquired(GeneralLicenseType_v2 jobLicense) {
            return Data.GeneralLocations[RandoCommonData.GetIDFromGeneralLicense(jobLicense)];
        }
        public void UnlockGarage(long id) {
            Data.HiddenGarages[id-RandoCommonData.AP_ID.GARAGES] = true;
        }
        

    }
#endregion
}
