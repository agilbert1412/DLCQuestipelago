using BepInEx.Logging;
using DLCLib.World;
using DLCQuestipelago.Archipelago;
using GameStateManagement;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch("msgBox_AcceptedTakeCoins")]
    public static class TriggerUtilBossDoorPatch
    {
        private const string SWORD_1 = "Big Sword Pack";
        private const string SWORD_2 = "Really Big Sword Pack";
        private const string SWORD_3 = "Unfathomable Sword Pack";

        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        //private static void msgBox_AcceptedTakeCoins(object sender, PlayerIndexEventArgs e)
        private static bool Prefix(object sender, PlayerIndexEventArgs e)
        {
            return _archipelago.HasReceivedItem(SWORD_1, out _) &&
                   _archipelago.HasReceivedItem(SWORD_2, out _) &&
                   _archipelago.HasReceivedItem(SWORD_3, out _);
        }
    }
}
