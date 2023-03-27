using DLCLib;
using DLCLib.Character;
using DLCLib.Conversation;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(FetchNPC))]
    [HarmonyPatch(nameof(FetchNPC.Activate))]
    public static class FetchNpcActivatePatch
    {
        private static Logger _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ConversationStarter _conversationStarter;

        public static void Initialize(Logger log, ArchipelagoClient archipelago, LocationChecker locationChecker, ConversationStarter conversationStarter)
        {
            _log = log;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _conversationStarter = conversationStarter;
        }

        //public override bool Activate()
        private static bool Prefix(FetchNPC __instance, ref bool __result)
        {
            if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
            {
                return true;
            }

            var hasSupplies = _archipelago.HasReceivedItem("Box of Various Supplies", out _);
            if (SceneManager.Instance.CurrentScene.EventList.Contains(ConversationManager.FETCH_QUEST_ACTIVATED_STR) && !__instance.FetchQuestStarted)
            {
                __result = _conversationStarter.StartConversation(__instance, "fetchqueststart");
                if (hasSupplies)
                {
                    SceneManager.Instance.CurrentScene.HUDManager.ObjectiveDisplay.MarkObjectivesComplete();
                }
            }
            else if (SceneManager.Instance.CurrentScene.EventList.Contains(ConversationManager.FETCH_QUEST_COMPLETE_STR))
            {
                __result = _conversationStarter.StartConversation(__instance, "fetchquestcomplete");
            }
            else if (hasSupplies)
            {
                __result = _conversationStarter.StartConversation(__instance, "fetchquestturnin");
            }
            else if (__instance.FetchQuestStarted)
            {
                __result = _conversationStarter.StartConversation(__instance, "fetchquestongoing");
            }
            else
            {
                __result = _conversationStarter.StartConversation(__instance, "default");
            }

            return false;
        }
    }
}
