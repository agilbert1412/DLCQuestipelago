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
            if (_locationChecker.IsLocationMissing("Sword"))
            {
                __instance.Enable();
                var IsCompleteField = typeof(Grindstone).GetProperty("IsComplete", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                IsCompleteField.SetValue(__instance, false);
            }
            return true;
        }

        private static void Postfix(Grindstone __instance, ref bool __result)
        {
            grindCount++;
            if (grindCount >= 9)
            {
                DLCManager.Instance.UnlockPack("grindstone");
            }
        }
    }
}
