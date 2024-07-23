using System;
using System.Diagnostics;
using Awardments;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Locations
{
    [HarmonyPatch(typeof(AwardmentUtil))]
    [HarmonyPatch("AwardKillComedianAwardment")]
    public static class IGetThatReferencePatch
    {
        private const string ACHIEVEMENT_NAME = $"I Get That Reference!";
        private static ILogger _logger;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, LocationChecker locationChecker)
        {
            _logger = logger;
            _locationChecker = locationChecker;
        }

        //internal static void AwardKillComedianAwardment()
        public static void Postfix()
        {
            try
            {
                _logger.LogInfo(ACHIEVEMENT_NAME);
                _locationChecker.AddCheckedLocation(ACHIEVEMENT_NAME);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(IGetThatReferencePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
