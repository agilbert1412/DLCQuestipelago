using Awardments;
using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.Character;
using DLCLib.Conversation;
using DLCLib.DLC;
using DLCLib.Scripts.LFOD;
using DLCLib.World;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(GrooveNPC))]
    [HarmonyPatch(nameof(GrooveNPC.Activate))]
    public static class GrooveNpcActivatePatch
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

            var hasCheckedPickaxeLocation = _locationChecker.IsLocationChecked("Pickaxe");
            Scene currentScene = Singleton<SceneManager>.Instance.CurrentScene;
            if (currentScene.EventList.Contains(ConversationManager.FETCH_QUEST_COMPLETE_STR))
            {
                if (hasCheckedPickaxeLocation)
                {
                    __instance.SetCurrentConversation("mattockcomplete");
                }
                else
                {
                    __instance.SetCurrentConversation("givemattock");
                    // AwardmentUtil.AwardGoodPlayerAwardment();
                }
            }
            else if (currentScene.EventList.Contains(TriggerUtil.ROCKS_DISCOVERED_STR))
            {
                __instance.SetCurrentConversation("fetchquest");
            }

            return;
        }
    }
}
