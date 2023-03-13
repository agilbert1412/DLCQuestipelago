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
    [HarmonyPatch(nameof(EndGame.OnBadEndingCreditsComplete))]
    public static class BadEndingObjectivePatch
    {
        private static ManualLogSource _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
            _objectivePersistence = objectivePersistence;
        }

        // public static void OnGoodEndingCreditsComplete()
        static void Postfix()
        {
            if (_objectivePersistence == null)
            {
                return;
            }

            _objectivePersistence.CompleteDlcBadEnding();
        }
    }
}
