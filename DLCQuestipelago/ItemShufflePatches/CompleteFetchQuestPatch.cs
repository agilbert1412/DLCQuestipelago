using System;
using System.Diagnostics;
using System.Reflection;
using DLCLib;
using DLCLib.Conversation;
using DLCQuestipelago.Archipelago;
using KaitoKid.ArchipelagoUtilities.Net;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(ConversationManager))]
    [HarmonyPatch(nameof(ConversationManager.FetchQuestComplete))]
    public static class CompleteFetchQuestPatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        //public static void FetchQuestComplete()
        private static bool Prefix()
        {
            try
            {
                if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Humble Indie Bindle");
                var currentScene = SceneManager.Instance.CurrentScene;
                var addEventMethod =
                    typeof(Scene).GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                addEventMethod.Invoke(currentScene, new object[] { ConversationManager.FETCH_QUEST_COMPLETE_STR });
                currentScene.HUDManager.ObjectiveDisplay.Enabled = false;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CompleteFetchQuestPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }

        }
    }
}
