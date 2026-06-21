using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using HarmonyLib;

namespace DvMod.Randomizer;
/// <summary>
/// Allows for deathlink (when activated, when a death occurs somewhere in AP multiworld, a deathlink is sent to all registered players)
/// In case of Derail Valley, "death" is derailing
/// </summary>
[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.SetCar))]
public class DeathLinkPatch {
    private static void SendDeathLink(TrainCar _) {
        Main.Player.deathLinkService!.SendDeathLink(new(Main.Player.Session.Players.ActivePlayer.Name, "derailed"));
    }
    public static void Derail(DeathLink _) => PlayerManager.Car?.Derail();
    public static void Prefix(TrainCar newCar) {
        if (!Main.IsConnected) return;
        if (newCar == PlayerManager.Car) return;
        if (!Main.Player.Config.DeathLink) return;
        if (newCar != null)
            newCar.OnDerailed += SendDeathLink;
        if (PlayerManager.Car != null)
            PlayerManager.Car.OnDerailed -= SendDeathLink;
    }
}