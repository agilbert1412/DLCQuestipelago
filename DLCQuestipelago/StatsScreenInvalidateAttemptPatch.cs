using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.Screens;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(StatsScreen))]
    [HarmonyPatch("IsValidAttempt")]
    public static class StatsScreenInvalidateAttemptPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // protected bool IsValidAttempt()
        public static bool Prefix(StatsScreen __instance, ref bool __result)
        {
            try
            {
                __result = false;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(StatsScreenInvalidateAttemptPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
