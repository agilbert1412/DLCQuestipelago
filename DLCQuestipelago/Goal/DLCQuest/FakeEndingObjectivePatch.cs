using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.Scripts;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using DLCQuestipelago.Serialization;
using HarmonyLib;

namespace DLCQuestipelago.Goal.DLCQuest
{
    [HarmonyPatch(typeof(EndGame))]
    [HarmonyPatch(nameof(EndGame.OnFakeEndingCreditsComplete))]
    public static class FakeEndingObjectivePatch
    {
        private static ManualLogSource _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
            _objectivePersistence = objectivePersistence;
        }

        // public static void OnFakeEndingCreditsComplete()
        static void Postfix()
        {
            _objectivePersistence.CompleteDlcFakeEnding();
        }
    }
}
