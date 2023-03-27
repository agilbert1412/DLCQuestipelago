using DLCLib.Conversation;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(ConversationManager))]
    [HarmonyPatch(nameof(ConversationManager.GiveMattock))]
    public static class GetPickaxePatch
    {
        private static Logger _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(Logger log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _log = log;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        //public static void GiveMattock()
        private static bool Prefix()
        {
            if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
            {
                return true; // run original logic
            }

            _locationChecker.AddCheckedLocation("Pickaxe");
            return false; // don't run original logic
        }
    }
}
