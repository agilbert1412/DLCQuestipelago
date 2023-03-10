using BepInEx.Logging;
using DLCLib;
using DLCLib.World.Props;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(Coin))]
    [HarmonyPatch(nameof(Coin.OnPickup))]
    public static class CoinPickupPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        //public override void OnPickup(Player player)
        static bool Prefix(Coin __instance, Player player)
        {
            // _log.LogInfo($"Picked up a coin! [{__instance.GetPhysicsObject().Position}] (Current: {player.Inventory.Coins}, Total:{player.Inventory.TotalCoinsCollected})");
            return true; // don't run original logic
        }

        static void Postfix(Coin __instance)
        {

        }
    }
}
