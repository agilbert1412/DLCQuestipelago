using BepInEx.Logging;
using DLCLib;
using DLCLib.Character;
using DLCLib.Conversation;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(FetchNPC))]
    [HarmonyPatch(nameof(FetchNPC.Activate))]
    public static class FetchNpcActivatePatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _log = log;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        //public override bool Activate()
        private static void Postfix(FetchNPC __instance, ref bool __result)
        {
            if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
            {
                return;
            }

            var hasSupplies = _archipelago.HasReceivedItem("Box of Various Supplies", out _);
            if (SceneManager.Instance.CurrentScene.EventList.Contains(ConversationManager.FETCH_QUEST_ACTIVATED_STR) && !__instance.FetchQuestStarted)
            {
                __instance.SetCurrentConversation("fetchqueststart");
                if (hasSupplies)
                {
                    SceneManager.Instance.CurrentScene.HUDManager.ObjectiveDisplay.MarkObjectivesComplete();
                }
            }
            else if (SceneManager.Instance.CurrentScene.EventList.Contains(ConversationManager.FETCH_QUEST_COMPLETE_STR))
            {
                __instance.SetCurrentConversation("fetchquestcomplete");
            }
            else if (hasSupplies)
            {
                __instance.SetCurrentConversation("fetchquestturnin");
            }
            else if (__instance.FetchQuestStarted)
            {
                __instance.SetCurrentConversation("fetchquestongoing");
            }
            else
            {
                __instance.SetCurrentConversation("default");
            }

            return;
        }
    }
}
