using BepInEx.Logging;
using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Serialization;
using HarmonyLib;

namespace DLCQuestipelago.Goal.LFOD
{
    [HarmonyPatch(typeof(BossFight))]
    [HarmonyPatch(nameof(BossFight.PerformFinalAttack))]
    public static class TrueEndingObjectivePatch
    {
        private static ManualLogSource _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
            _objectivePersistence = objectivePersistence;
        }

        // public static void PerformFinalAttack()
        static void Postfix()
        {
            if (!Plugin.Instance.HasEnteredGame)
            {
                return;
            }

            _objectivePersistence.CompleteLfodTrueEnding();
        }
    }
}
