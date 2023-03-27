using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Archipelago;
using GameStateManagement;
using HarmonyLib;

namespace DLCQuestipelago.FakeEndingBehavior
{
    [HarmonyPatch(typeof(FakeEnding))]
    [HarmonyPatch("reconnectingScreen3_Accepted")]
    public static class SuccessReconnectPatch
    {
        private static Logger _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(Logger log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        //private static void reconnectingScreen3_Accepted(object sender, PlayerIndexEventArgs e)
        public static void Postfix(object sender, PlayerIndexEventArgs e)
        {
            _archipelago.ReconnectAfterTemporaryDisconnect();
        }
    }
}
