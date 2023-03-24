using BepInEx.Logging;
using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Archipelago;
using GameStateManagement;
using HarmonyLib;

namespace DLCQuestipelago.FakeEndingBehavior
{
    [HarmonyPatch(typeof(FakeEnding))]
    [HarmonyPatch("reconnectingScreen_Accepted")]
    public static class FailedReconnectPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        //private static void reconnectingScreen_Accepted(object sender, PlayerIndexEventArgs e)
        public static void Postfix(object sender, PlayerIndexEventArgs e)
        {
            _archipelago.MakeSureConnected(0);
        }
    }
}
