using System;
using System.Diagnostics;
using DLCLib.Scripts;
using DLCQuestipelago.Serialization;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Goal.DLCQuest
{
    [HarmonyPatch(typeof(EndGame))]
    [HarmonyPatch(nameof(EndGame.OnFakeEndingCreditsComplete))]
    public static class FakeEndingObjectivePatch
    {
        private static ILogger _logger;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ILogger logger, ObjectivePersistence objectivePersistence)
        {
            _logger = logger;
            _objectivePersistence = objectivePersistence;
        }

        // public static void OnFakeEndingCreditsComplete()
        private static void Postfix()
        {
            try
            {
                if (_objectivePersistence == null)
                {
                    return;
                }

                _objectivePersistence.CompleteDlcFakeEnding();
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
