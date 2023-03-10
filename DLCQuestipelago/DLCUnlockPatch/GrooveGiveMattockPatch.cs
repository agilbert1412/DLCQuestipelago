using System.Reflection;
using Awardments;
using BepInEx.Logging;
using DLCLib;
using DLCLib.Character;
using DLCLib.Conversation;
using DLCLib.World;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(GrooveNPC))]
    [HarmonyPatch(nameof(GrooveNPC.Activate))]
    public static class GrooveGiveMattockPatch
    {
        private static ManualLogSource _log;
        private static LocationChecker _locationChecker;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
        }

        //public override bool Activate()
        static bool Prefix(GrooveNPC __instance, ref bool __result)
        {
            Scene currentScene = SceneManager.Instance.CurrentScene;
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
