using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Scripts;
using DLCQuestipelago.Serialization;
using HarmonyLib;

namespace DLCQuestipelago.Goal.DLCQuest
{
    [HarmonyPatch(typeof(EndGame))]
    [HarmonyPatch(nameof(EndGame.OnBadEndingCreditsComplete))]
    public static class BadEndingObjectivePatch
    {
        private static ManualLogSource _log;
        private static ObjectivePersistence _objectivePersistence;

        public static void Initialize(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(BadEndingObjectivePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
