using BepInEx.Logging;
using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Archipelago;
using HarmonyLib;

namespace DLCQuestipelago.FakeEndingBehavior
{
    [HarmonyPatch(typeof(FakeEnding))]
    [HarmonyPatch(nameof(FakeEnding.OnFakeEndingComplete))]
    public static class FakeEndingDisconnectPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        //public static void OnFakeEndingComplete()
        public static void Postfix()
        {
            _archipelago.DisconnectTemporarily();
        }
    }
}
