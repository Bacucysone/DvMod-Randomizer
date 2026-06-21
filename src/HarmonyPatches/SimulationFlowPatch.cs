using HarmonyLib;
using LocoSim.Implementations;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(SimulationFlow))]
public static class SimulationFlowPatch {
    /// <summary>
    /// Adding frame update behaviour: only caller of <see cref="RandoPlayer.CallUpdate"/>
    /// </summary>
    [HarmonyPostfix, HarmonyPatch("Tick")]
    public static void Tick_Postfix() {
        if (Main.IsConnected) Main.Player.CallUpdate();
    }
}