using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Serialization;
using HarmonyLib;

namespace DLCQuestipelago.Goal.LFOD
{
    [HarmonyPatch(typeof(BossFight))]
    [HarmonyPatch(nameof(BossFight.PerformFinalAttack))]
    public static class TrueEndingObjectivePatch
    {
        private static Logger _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(Logger log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
            _objectivePersistence = objectivePersistence;
        }

        // public static void PerformFinalAttack()
        private static void Postfix()
        {
            if (_objectivePersistence == null)
            {
                return;
            }

            _objectivePersistence.CompleteLfodTrueEnding();
        }
    }
}
