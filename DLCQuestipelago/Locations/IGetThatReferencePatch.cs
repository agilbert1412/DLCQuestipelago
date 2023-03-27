using Awardments;
using HarmonyLib;

namespace DLCQuestipelago.Locations
{
    [HarmonyPatch(typeof(AwardmentUtil))]
    [HarmonyPatch("AwardKillComedianAwardment")]
    public static class IGetThatReferencePatch
    {
        private const string ACHIEVEMENT_NAME = $"I Get That Reference!";
        private static Logger _log;
        private static LocationChecker _locationChecker;

        public static void Initialize(Logger log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
        }

        //internal static void AwardKillComedianAwardment()
        public static void Postfix()
        {
            _log.LogInfo(ACHIEVEMENT_NAME);
            _locationChecker.AddCheckedLocation(ACHIEVEMENT_NAME);
        }
    }
}
