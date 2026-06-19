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

namespace DvMod.Randomizer
{

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
    public class DVConfig(int[] shuntThreshold, int[] freightThreshold, int[] locoJobsThreshold, int victory, int victoryThreshold, bool hintsOnLocoLicense, bool hintsOnStationLicense, bool deathLink) {
                public int[] ShuntThreshold = shuntThreshold;
                public int[] FreightThreshold = freightThreshold;
                public int[] LocoJobsThreshold = locoJobsThreshold;
                public int Victory=victory;
                public int VictoryThreshold = victoryThreshold;
                public bool HintsOnLocoLicense = hintsOnLocoLicense;
                public bool HintsOnStationLicense = hintsOnStationLicense;
                public bool DeathLink = deathLink;
            }
    public class RandoSaveData(
        int version,
        bool[] stationLicenses, 
        bool[] hiddenGarages, 
        bool[] jobLocations,
        bool[] generalLocations,
        bool[] locoLocations,
        int[] receivedRelics, 
        int index, 
        int[] shunts, 
        int[] freights, 
        int[] locoJobs, 
        bool alreadyWon,
        HashSet<long> locationsChecked,
        DVConfig config,
        int tokens,
        string serverName,
        int port,
        string slotName,
        string password
        ) {
            
        public bool[] StationLicenses = stationLicenses;
        public bool[] HiddenGarages = hiddenGarages;
        public bool[] JobLocations = jobLocations;
        public bool[] GeneralLocations = generalLocations;
        public bool[] LocoLocations = locoLocations;
        public int[] ReceivedRelics = receivedRelics;
        public int[] Shunts = shunts;
        public int Index = index;
        public int[] Freights = freights;
        public int[] LocoJobs = locoJobs;
        public bool AlreadyWon = alreadyWon;
        public int Version = version;
        public HashSet<long> LocationsChecked = locationsChecked;
        public DVConfig Config = config;
        public int Tokens = tokens;
        public string ServerName = serverName;
        public int Port = port;
        public string SlotName = slotName;
        public string Password = password;
        public static RandoSaveData CreateSaveData(DVConfig config) => new(
            Main.VERSION,
            new bool[20],
            new bool[4],
            new bool[12],
            new bool[13],
            new bool[57],
            new int[6],
            0,
            new int[20],
            new int[20],
            new int[6],
            false,
            [],
            config,
            0,
            Main.Settings!.serverName,
            Main.Settings.Port,
            Main.Settings.User,
            Main.Settings.Password
        );
    }
    
    public class RandoPlayer
    {
        internal class DemoLocoListener(int idx, float spatialThreshold = 5f, float timeThreshold = 20f) {
            private readonly Vector3 _locoPosition = RandoCommonData.GetInfoRestorationFromLocoLocationOrder(idx);
            private readonly long _checkId = RandoCommonData.AP_ID.LOC_LOCO_RESTORATION + idx;
            private float _lastTime;
            public void CheckPosition() {
                if (PlayerManager.PlayerTransform == null) return;
                if (!(Time.time - _lastTime > timeThreshold) ||
                    !((PlayerManager.PlayerTransform.AbsolutePosition() - _locoPosition).magnitude <
                      spatialThreshold)) return;
                string stationNeeded = RandoCommonData.GetStationFromLocoLocations(_locoPosition);
                bool stationOk = Main.Player.GotStationLicense(stationNeeded);
                bool museumOk = SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2());
                if (stationOk && museumOk) {
                    ItemInfo item = Main.Player.UnlockCheck(_checkId);
                    Main.Player.CheckRestoLoco(_checkId);
                    Main.NotifyPlayer($"You found a {item.ItemDisplayName} for {item.Player.Name} on the ground!");
                    Main.Player.UpdateEvent -= CheckPosition;
                } else{
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
        public DVConfig Config {get => Data.Config;}
        private readonly ConcurrentQueue<DV_APItem> _waitingQueue = new();
        private static PauseMenu Menu => UnityEngine.Object.FindObjectOfType<PauseMenu>();
        public ArchipelagoSession Session;
        public APSlotData SlotData {get;}
        public event Action UpdateEvent;
        public DeathLinkService deathLinkService ;
        

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
        }
        private IEnumerator Subscribe() {
            while (Menu == null) yield return null;
            Menu.controller.ExitLevelRequested += Main.Disconnect;
            Menu.controller.QuitGameRequested += Main.Disconnect;
        }
        public RandoPlayer(RandoSaveData saveData) {
            bool useGivenAuth = saveData == null || Main.Settings!.ForceUseSave;
            (string server, string password, string slotName, int port) = useGivenAuth ?
                    (Main.Settings!.serverName, Main.Settings.Password, Main.Settings.User, Main.Settings.Port):
                    (saveData!.ServerName, saveData.Password, saveData.SlotName, saveData.Port);
            Session = ArchipelagoSessionFactory.CreateSession(server, port);
            LoginResult login = Session.TryConnectAndLogin("Derail Valley", slotName, ItemsHandlingFlags.AllItems, password: password);
            if (login is LoginFailure failLogin) {
                Main.Log("Error! We got the following error while connecting: "+failLogin.Errors.Aggregate((acc, s) => acc+"/"+s));
                if (useGivenAuth)
                    Main.NotifyPlayer("Archipelago server connection failed. Please check that the server is up and running and that you provided the correct connection information.");
                else
                    Main.NotifyPlayer($"The stored connection information do not work. Please verify your server, if any connection data has changed, provide the correct ones in the mod menu options and press the \"Use the provided credential authentication\" button\nLast known information for this file: {server}:{port}, Slot Name: {slotName}/Password: {password}");
                MainMenu.GoBackToMainMenu();
                throw new Exception();
            }
            SlotData = ((LoginSuccessful)login).SlotData;
            SingletonBehaviour<CoroutineManager>.Instance.Run(Subscribe());
            Data = saveData ?? RandoSaveData.CreateSaveData(SlotData.Config);
            Data.ServerName = server;
            Data.Password = password;
            Data.SlotName = slotName;
            Data.Port = port;
            if (Data.Config.DeathLink) {
                deathLinkService = Session.CreateDeathLinkService();
                deathLinkService.OnDeathLinkReceived += DeathLinkPatch.Derail;
                deathLinkService.EnableDeathLink();
            }

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
        private async void ProcessItems() {
            if (_waitingQueue.TryDequeue(out var item)){
                await item.Acquire();
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
        public int AddRelic(long id) {
            return ++Data.ReceivedRelics[id-RandoCommonData.AP_ID.RELIC];
        }
        public void AcquireLicense(string station) {
            Data.StationLicenses[RandoCommonData.GetOrderFromStationName(station)] = true;
        }
        public (int, ItemInfo) FinishLoco(TrainCar car) {
            if (car == null) return (-1, null);
            int locoIdx = RandoCommonData.GetOrderFromLocoType(car.carType);
            int remaining = Data.Config.LocoJobsThreshold[locoIdx] - ++Data.LocoJobs[locoIdx];
            ItemInfo item = remaining == 0 ? UnlockCheck(0x600+locoIdx) : null;
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
            Data.Shunts[stOrder] += 1;
            int remaining = Data.Config.ShuntThreshold[stOrder] - Data.Shunts[stOrder];
            if (remaining >= 0) {
                return (remaining, UnlockCheck(0x2000 + stOrder * 0x100 + Data.Shunts[stOrder] - 1));
            }
            return (Math.Max(remaining,0), null);
        }
        public (int, ItemInfo) FinishTransport(string station) {
            int stOrder = RandoCommonData.GetOrderFromStationName(station);
            Data.Freights[stOrder] += 1;
            int remaining = Data.Config.FreightThreshold[stOrder] - Data.Freights[stOrder];
            if (remaining >= 0) {
                return (remaining, UnlockCheck(0x4000 + stOrder * 0x100 + Data.Freights[stOrder] - 1));
            }
            return (Math.Max(0,remaining), null);
        }

        #endregion
        #region Checking player possibilities
        private bool HasChecked<T>(Func<T, int> f, T value, bool[] map) {
            int id = f(value);
            if (id < 0) return true;
            return map[id];
        }
        public bool HasChecked(JobLicenseType_v2 jobLicense) {
            return HasChecked(x => RandoCommonData.GetIDFromJobLicense(x).Item2, jobLicense,  Data.JobLocations);
        }
        public bool HasChecked(GeneralLicenseType_v2 generalLicense) {
            return HasChecked(x => RandoCommonData.GetIDFromGeneralLicense(x).Item2, generalLicense, Data.GeneralLocations);
        }

        public bool GotStationLicense(string name) {
            return Data.StationLicenses[RandoCommonData.GetOrderFromStationName(name)];
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
        
        public void UnlockGarage(long id) {
            Data.HiddenGarages[id-RandoCommonData.AP_ID.GARAGES] = true;
        }
        public void CheckRestoLoco(long id) {
            Data.LocoLocations[id - RandoCommonData.AP_ID.LOC_LOCO_RESTORATION] = true;
        }
        public void CheckGLicense(long id) {
            Data.GeneralLocations[id - RandoCommonData.AP_ID.LOC_GENERAL_LICENSES] = true;
        }
        public void CheckJLicense(long id) {
            Data.JobLocations[id - RandoCommonData.AP_ID.LOC_JOB_LICENSES] = true;
        }

    }
#endregion
}
