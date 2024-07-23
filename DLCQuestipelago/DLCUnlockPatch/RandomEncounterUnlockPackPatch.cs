using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.DLC;
using DLCLib.Scripts;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(RandomEncounter))]
    [HarmonyPatch(nameof(RandomEncounter.StartRandomEncounterConversation))]
    public static class RandomEncounterUnlockPackPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
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
                _logger.LogError($"Failed in {nameof(RandomEncounterUnlockPackPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
