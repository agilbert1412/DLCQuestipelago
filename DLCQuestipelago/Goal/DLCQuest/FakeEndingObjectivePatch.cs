using DLCLib.Scripts;
using DLCQuestipelago.Serialization;
using HarmonyLib;

namespace DLCQuestipelago.Goal.DLCQuest
{
    [HarmonyPatch(typeof(EndGame))]
    [HarmonyPatch(nameof(EndGame.OnFakeEndingCreditsComplete))]
    public static class FakeEndingObjectivePatch
    {
        private static Logger _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(Logger log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
            _objectivePersistence = objectivePersistence;
        }

        // public static void OnFakeEndingCreditsComplete()
        private static void Postfix()
        {
            if (_objectivePersistence == null)
            {
                return;
            }

            _objectivePersistence.CompleteDlcFakeEnding();
        }
    }
}
