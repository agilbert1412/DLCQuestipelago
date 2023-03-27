using Awardments;
using HarmonyLib;

namespace DLCQuestipelago.Locations
{
    [HarmonyPatch(typeof(AwardmentUtil))]
    [HarmonyPatch("AwardTriedToCheatAwardment")]
    public static class NiceTryPatch
    {
        private const string ACHIEVEMENT_NAME = $"Nice Try";
        private static Logger _log;
        private static LocationChecker _locationChecker;

        public static void Initialize(Logger log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
        }

        //internal static void AwardTriedToCheatAwardment()
        public static void Postfix()
        {
            _log.LogInfo(ACHIEVEMENT_NAME);
            _locationChecker.AddCheckedLocation(ACHIEVEMENT_NAME);
        }
    }
}
