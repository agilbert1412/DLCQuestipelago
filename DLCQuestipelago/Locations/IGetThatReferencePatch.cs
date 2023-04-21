using System;
using System.Diagnostics;
using Awardments;
using BepInEx.Logging;
using HarmonyLib;

namespace DLCQuestipelago.Locations
{
    [HarmonyPatch(typeof(AwardmentUtil))]
    [HarmonyPatch("AwardKillComedianAwardment")]
    public static class IGetThatReferencePatch
    {
        private const string ACHIEVEMENT_NAME = $"I Get That Reference!";
        private static ManualLogSource _log;
        private static LocationChecker _locationChecker;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
        }

        //internal static void AwardKillComedianAwardment()
        public static void Postfix()
        {
            try
            {
                _log.LogInfo(ACHIEVEMENT_NAME);
                _locationChecker.AddCheckedLocation(ACHIEVEMENT_NAME);
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(IGetThatReferencePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
