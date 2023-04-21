using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Serialization;
using HarmonyLib;

namespace DLCQuestipelago.Goal.LFOD
{
    [HarmonyPatch(typeof(FakeEnding))]
    [HarmonyPatch(nameof(FakeEnding.SetupAfterFakeEnding))]
    public static class FakeEndingObjectivePatch
    {
        private static ManualLogSource _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(FakeEndingObjectivePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
