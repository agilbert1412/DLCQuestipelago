using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Scripts;
using DLCQuestipelago.Serialization;
using HarmonyLib;

namespace DLCQuestipelago.Goal.DLCQuest
{
    [HarmonyPatch(typeof(EndGame))]
    [HarmonyPatch(nameof(EndGame.OnFakeEndingCreditsComplete))]
    public static class FakeEndingObjectivePatch
    {
        private static ManualLogSource _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(FakeEndingObjectivePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
