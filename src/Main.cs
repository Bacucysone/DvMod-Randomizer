using HarmonyLib;
using UnityModManagerNet;


namespace DvMod.Randomizer
{

    /*[HarmonyPatch(typeof(DevUtil), nameof(DevUtil.IsDevMachine))]
    public static class DevPatch {
        public static void Postfix(ref bool __result) => __result = true;
    }*/

    public class Settings : UnityModManager.ModSettings, IDrawable {
        [Draw("Server address")] public string serverName = "localhost";
        [Draw("Port")] public int Port = 38281;
        [Draw("Slot name (Must correspond to the name given to the Archipelago Server)")] public string User="";
        [Draw("Password (leave blank if no password)")] public string Password = "";
        public override void Save(UnityModManager.ModEntry mod){
            Save(this, mod);
        }
        public void OnChange(){}
    }

    public class Main 
    {
        public static Settings? settings;
        public static UnityModManager.ModEntry? mod;
        public static RandoPlayer? player;
        public static void Load(UnityModManager.ModEntry modEntry)
        {
            settings = Settings.Load<Settings>(modEntry);
            mod = modEntry;
            mod.OnToggle = OnToggle;
            mod.OnGUI += OnGUI;
            mod.OnSaveGUI += OnSaveGUI;

        }
        public static void OnGUI(UnityModManager.ModEntry modEntry) {
            settings!.Draw(modEntry);
        }
        public static void OnSaveGUI(UnityModManager.ModEntry modEntry) {
            settings!.Save(modEntry);
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
            mod!.Logger.Log(message);
        }
        public static void Error(string message) {
            mod!.Logger.Error(message);
        }
    }


}
