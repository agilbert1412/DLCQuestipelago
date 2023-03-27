using DLCLib;
using DLCLib.Character;
using DLCLib.Conversation;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(GrooveNPC))]
    [HarmonyPatch(nameof(GrooveNPC.Activate))]
    public static class GrooveGiveMattockPatch
    {
        private static Logger _log;
        private static LocationChecker _locationChecker;

        public static void Initialize(Logger log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
        }

        //public override bool Activate()
        private static bool Prefix(GrooveNPC __instance, ref bool __result)
        {
            var currentScene = SceneManager.Instance.CurrentScene;
            if (currentScene.EventList.Contains(ConversationManager.FETCH_QUEST_COMPLETE_STR) && _locationChecker.IsLocationMissing("Pickaxe"))
            {
                __instance.SetCurrentConversation("givemattock");
                // AwardmentUtil.AwardGoodPlayerAwardment();
                currentScene.ConversationManager.StartConversation(__instance.CurrentConversation);
                // SceneManager.Instance.CurrentScene.Player.GetPhysicsObject().ResetDynamics();

                return false; // don't run original logic
            }

            return true; // run original logic 
        }
    }
}
