
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using HarmonyLib;

namespace DvMod.Randomizer {
    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.SetCar))]
    public class DeathLinkPatch {
        private static void SendDeathLink(TrainCar _) {
            Main.player!.deathLinkService!.SendDeathLink(new(Main.player.Session.Players.ActivePlayer.Name, "derailed"));
        }
        public static void Derail(DeathLink _) => PlayerManager.Car?.Derail();
        public static void Prefix(TrainCar newCar, TrainCar ___Car) {
            if (Main.player == null) return;
            if (newCar == ___Car) return;
            if (!Main.player.Data.Config.DeathLink) return;
            newCar.OnDerailed += SendDeathLink;
            ___Car.OnDerailed -= SendDeathLink;
        }
    }
}