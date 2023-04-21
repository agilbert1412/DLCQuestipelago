using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Conversation;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(ConversationManager))]
    [HarmonyPatch(nameof(ConversationManager.GrantWoodenSword))]
    public static class GrantWoodenSwordPatch
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

        //public static void GrantWoodenSword()
        private static bool Prefix()
        {
            try
            {
                if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Wooden Sword");
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(GrantWoodenSwordPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
