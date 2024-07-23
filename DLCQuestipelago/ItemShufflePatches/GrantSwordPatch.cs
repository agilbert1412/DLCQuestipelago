using System;
using System.Diagnostics;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using KaitoKid.ArchipelagoUtilities.Net;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(Grindstone))]
    [HarmonyPatch("GrantSword")]
    public static class GrantSwordPatch
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

        //private void GrantSword()
        private static bool Prefix()
        {
            try
            {
                if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Sword");
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GrantSwordPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
