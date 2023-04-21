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
    [HarmonyPatch(nameof(ConversationManager.GrantGun))]
    public static class GrantGunPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago,
            LocationChecker locationChecker)
        {
            _log = log;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        //public static void GrantGun()
        private static bool Prefix()
        {
            try
            {
                if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Gun");
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(GrantGunPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}