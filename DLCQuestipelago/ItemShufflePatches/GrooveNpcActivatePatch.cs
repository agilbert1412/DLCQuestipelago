using Core;
using DLCLib;
using DLCLib.Character;
using DLCLib.World;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(GrooveNPC))]
    [HarmonyPatch(nameof(GrooveNPC.Activate))]
    public static class GrooveNpcActivatePatch
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

            var hasCheckedPickaxeLocation = _locationChecker.IsLocationChecked("Pickaxe");
            Scene currentScene = Singleton<SceneManager>.Instance.CurrentScene;
            if (currentScene.Player.Inventory.HasBindle && !hasCheckedPickaxeLocation/*currentScene.EventList.Contains(ConversationManager.FETCH_QUEST_COMPLETE_STR)*/)
            {
                __result = _conversationStarter.StartConversation(__instance, "givemattock");
            }
            else if (!hasCheckedPickaxeLocation && currentScene.EventList.Contains(TriggerUtil.ROCKS_DISCOVERED_STR))
            {
                __result = _conversationStarter.StartConversation(__instance, "fetchquest");
            }
            else if (hasCheckedPickaxeLocation)
            {
                __result = _conversationStarter.StartConversation(__instance, "mattockcomplete");
                // AwardmentUtil.AwardGoodPlayerAwardment();
            }
            else
            {
                __result = _conversationStarter.StartConversation(__instance, "default");
            }

            return false;
        }
    }
}
