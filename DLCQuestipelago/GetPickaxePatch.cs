using BepInEx.Logging;
using DLCLib.Conversation;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(ConversationManager))]
    [HarmonyPatch(nameof(ConversationManager.GiveMattock))]
    public static class GetPickaxePatch
    {
        private static ManualLogSource _log;
        private static LocationChecker _locationChecker;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
        }

        //public static void GiveMattock()
        private static bool Prefix()
        {
            _locationChecker.AddCheckedLocation("Pickaxe");
            return false; // don't run original logic
        }
    }
}
