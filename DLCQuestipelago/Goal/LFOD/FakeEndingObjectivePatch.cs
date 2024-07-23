using System;
using System.Diagnostics;
using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Serialization;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Goal.LFOD
{
    [HarmonyPatch(typeof(FakeEnding))]
    [HarmonyPatch(nameof(FakeEnding.SetupAfterFakeEnding))]
    public static class FakeEndingObjectivePatch
    {
        private static ILogger _logger;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ILogger logger, ObjectivePersistence objectivePersistence)
        {
            _logger = logger;
            _objectivePersistence = objectivePersistence;
        }

        // public static void SetupAfterFakeEnding()
        private static void Postfix()
        {
            try
            {
                if (_objectivePersistence == null)
                {
                    return;
                }

                _objectivePersistence.CompleteLfodFakeEnding();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FakeEndingObjectivePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
