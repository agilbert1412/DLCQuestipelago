using System;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(Grindstone))]
    [HarmonyPatch(nameof(Grindstone.Activate))]
    public static class GrindstoneUnlockPackPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static int grindCount;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _log = log;
            grindCount = 0;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public bool Activate()
        public static bool Prefix(Grindstone __instance, ref bool __result)
        {
            try
            {
                if (_locationChecker.IsLocationMissing("Sword"))
                {
                    __instance.Enable();
                    var IsCompleteField = typeof(Grindstone).GetProperty("IsComplete",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    IsCompleteField.SetValue(__instance, false);
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(GrindstoneUnlockPackPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }

        private static void Postfix(Grindstone __instance, ref bool __result)
        {
            try
            {
                grindCount++;
                if (grindCount >= 9)
                {
                    DLCManager.Instance.UnlockPack("grindstone");
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(GrindstoneUnlockPackPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
