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
        private static LocationChecker _locationChecker;
        private static ItemParser _itemParser;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker, ItemParser itemParser)
        {
            _log = log;
            _locationChecker = locationChecker;
            _itemParser = itemParser;
        }

        // protected bool IsValidAttempt()
        public static bool Prefix(StatsScreen __instance, ref bool __result)
        {
            __result = false;
            return false; // don't run original logic
        }
    }
}
