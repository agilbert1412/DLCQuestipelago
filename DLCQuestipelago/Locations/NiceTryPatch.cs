using System;
using System.Diagnostics;
using Awardments;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Locations
{
    [HarmonyPatch(typeof(AwardmentUtil))]
    [HarmonyPatch("AwardTriedToCheatAwardment")]
    public static class NiceTryPatch
    {
        private const string ACHIEVEMENT_NAME = $"Nice Try";
        private static ILogger _logger;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, LocationChecker locationChecker)
        {
            _logger = logger;
            _locationChecker = locationChecker;
        }

        //internal static void AwardTriedToCheatAwardment()
        public static void Postfix()
        {
            try
            {
                _logger.LogInfo(ACHIEVEMENT_NAME);
                _locationChecker.AddCheckedLocation(ACHIEVEMENT_NAME);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(NiceTryPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
