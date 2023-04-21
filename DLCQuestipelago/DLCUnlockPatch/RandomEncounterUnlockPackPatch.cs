using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.Scripts;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(RandomEncounter))]
    [HarmonyPatch(nameof(RandomEncounter.StartRandomEncounterConversation))]
    public static class RandomEncounterUnlockPackPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // public static void StartRandomEncounterConversation()
        private static void Postfix()
        {
            try
            {
                DLCManager.Instance.UnlockPack("psychological");
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(RandomEncounterUnlockPackPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
