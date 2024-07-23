using System;
using System.Diagnostics;
using DLCLib;
using DLCLib.Character;
using DLCLib.Conversation;
using KaitoKid.ArchipelagoUtilities.Net;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(GrooveNPC))]
    [HarmonyPatch(nameof(GrooveNPC.Activate))]
    public static class GrooveGiveMattockPatch
    {
        private static ILogger _logger;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, LocationChecker locationChecker)
        {
            _logger = logger;
            _locationChecker = locationChecker;
        }

        //public override bool Activate()
        private static bool Prefix(GrooveNPC __instance, ref bool __result)
        {
            try
            {
                var currentScene = SceneManager.Instance.CurrentScene;
                if (currentScene.EventList.Contains(ConversationManager.FETCH_QUEST_COMPLETE_STR) &&
                    _locationChecker.IsLocationMissing("Pickaxe"))
                {
                    __instance.SetCurrentConversation("givemattock");
                    // AwardmentUtil.AwardGoodPlayerAwardment();
                    currentScene.ConversationManager.StartConversation(__instance.CurrentConversation);
                    // SceneManager.Instance.CurrentScene.Player.GetPhysicsObject().ResetDynamics();

                    return false; // don't run original logic
                }

                return true; // run original logic 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GrooveGiveMattockPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
