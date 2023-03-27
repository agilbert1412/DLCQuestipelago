﻿using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(Grindstone))]
    [HarmonyPatch("GrantSword")]
    public static class GrantSwordPatch
    {
        private static Logger _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(Logger log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _log = log;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        //private void GrantSword()
        private static bool Prefix()
        {
            if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
            {
                return true; // run original logic
            }

            _locationChecker.AddCheckedLocation("Sword");
            return false; // don't run original logic
        }
    }
}
