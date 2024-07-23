using System;
using System.Diagnostics;
using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Serialization;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Goal.LFOD
{
    [HarmonyPatch(typeof(BossFight))]
    [HarmonyPatch(nameof(BossFight.PerformFinalAttack))]
    public static class TrueEndingObjectivePatch
    {
        private static ILogger _logger;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ILogger logger, ObjectivePersistence objectivePersistence)
        {
            _logger = logger;
            _objectivePersistence = objectivePersistence;
        }

        // public static void PerformFinalAttack()
        private static void Postfix()
        {
            try
            {
                if (_objectivePersistence == null)
                {
                    return;
                }

                _objectivePersistence.CompleteLfodTrueEnding();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TrueEndingObjectivePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
