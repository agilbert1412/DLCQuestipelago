using System;
using System.Diagnostics;
using BepInEx.Logging;
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
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ConversationStarter _conversationStarter;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker, ConversationStarter conversationStarter)
        {
            _log = log;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _conversationStarter = conversationStarter;
        }

        //public override bool Activate()
        private static bool Prefix(FetchNPC __instance, ref bool __result)
        {
            try
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
                    // It was going here
                    __result = _conversationStarter.StartConversation(__instance, "fetchquestcomplete");
                }
                else if (hasSupplies)
                {
                    //instead of here
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
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(FetchNpcActivatePatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
