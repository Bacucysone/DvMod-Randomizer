using System;
using DV.UI;
using DV.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using UnityModManagerNet;


namespace DvMod.Randomizer
{
    public class Settings : UnityModManager.ModSettings, IDrawable {
        [Draw("Server address")] public string serverName = "localhost";
        [Draw("Port")] public int Port = 38281;
        [Draw("Slot name (Must correspond to the name given to the Archipelago Server)")] public string User="";
        [Draw("Password (leave blank if no password)")] public string Password = "";
        [Draw("Create a new Archipelago save on new career save?")]public bool CreateAPSave = false;
        [Obsolete] [Draw("When continuing a file, connection information are stored in the file. Set this to true to use the provided authentication credentials instead.")]public bool ForceUseSave = false;
        public override void Save(UnityModManager.ModEntry mod){
            Save(this, mod);
        }
        public void OnChange(){}
    }

    public class Main {
        public const int VERSION = 2;
        public static Settings Settings;
        public static UnityModManager.ModEntry Mod;
        // ReSharper disable once InconsistentNaming
        private static RandoPlayer _player;
        public static RandoPlayer Player => _player ?? throw new NullReferenceException();
        public static bool IsConnected => _player != null;

        public static void Connect(RandoSaveData saveData) {
            if (IsConnected) _player.Dispose();
            _player = new RandoPlayer(saveData);
        }
        public static void Disconnect() {
            if (!IsConnected) return;
            _player.Dispose();
            _player = null;
        }
        [UsedImplicitly]
        public static void Load(UnityModManager.ModEntry modEntry)
        {
            Settings = Settings.Load<Settings>(modEntry);
            Mod = modEntry;
            Mod.OnToggle = OnToggle;
            Mod.OnGUI += OnGUI;
            Mod.OnSaveGUI += OnSaveGUI;
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry) {
            Settings!.Draw(modEntry);
        }
        public static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            Settings!.Save(modEntry);
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Harmony harmony = new(modEntry.Info.Id);

            if (value)
            {
                harmony.PatchAll();
            }
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        }

        public static void Log(string message) {
            Mod!.Logger.Log(message);
        }
        public static void Error(string message) {
            Mod!.Logger.Error(message);
        }
        public static void NotifyPlayer(string message) {
            SingletonBehaviour<ACanvasController<CanvasController.ElementType>>.Instance.NotificationManager.ShowNotification(
                message,
                duration: 5f,
                localize: false
            );
        }
    }


}
