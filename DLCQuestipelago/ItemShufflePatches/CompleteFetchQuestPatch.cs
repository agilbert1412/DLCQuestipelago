﻿using System;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using DLCLib;
using DLCLib.Conversation;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(ConversationManager))]
    [HarmonyPatch(nameof(ConversationManager.FetchQuestComplete))]
    public static class CompleteFetchQuestPatch
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
                _log.LogError($"Failed in {nameof(CompleteFetchQuestPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }

        }
    }
}
