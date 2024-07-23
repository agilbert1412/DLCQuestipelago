using System;
using System.Diagnostics;
using DLCLib.Scripts;
using DLCQuestipelago.Serialization;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Goal.DLCQuest
{
    [HarmonyPatch(typeof(EndGame))]
    [HarmonyPatch(nameof(EndGame.OnBadEndingCreditsComplete))]
    public static class BadEndingObjectivePatch
    {
        private static ILogger _logger;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ILogger logger, ObjectivePersistence objectivePersistence)
        {
            _logger = logger;
            _objectivePersistence = objectivePersistence;
        }

        // public static void OnGoodEndingCreditsComplete()
        private static void Postfix()
        {
            try
            {
                if (_objectivePersistence == null)
                {
                    return;
                }

                _objectivePersistence.CompleteDlcBadEnding();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(BadEndingObjectivePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
