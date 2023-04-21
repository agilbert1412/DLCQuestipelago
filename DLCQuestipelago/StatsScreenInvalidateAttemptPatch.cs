using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Screens;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(StatsScreen))]
    [HarmonyPatch("IsValidAttempt")]
    public static class StatsScreenInvalidateAttemptPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(StatsScreenInvalidateAttemptPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
