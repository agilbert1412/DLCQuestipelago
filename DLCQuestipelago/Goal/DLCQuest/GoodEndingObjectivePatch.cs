using DLCLib.Scripts;
using DLCQuestipelago.Serialization;
using HarmonyLib;

namespace DLCQuestipelago.Goal.DLCQuest
{
    [HarmonyPatch(typeof(EndGame))]
    [HarmonyPatch(nameof(EndGame.OnGoodEndingCreditsComplete))]
    public static class GoodEndingObjectivePatch
    {
        private static Logger _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(Logger log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
            _objectivePersistence = objectivePersistence;
        }

        // public static void OnGoodEndingCreditsComplete()
        private static void Postfix()
        {
            if (_objectivePersistence == null)
            {
                return;
            }

            _objectivePersistence.CompleteDlcGoodEnding();
        }
    }
}
