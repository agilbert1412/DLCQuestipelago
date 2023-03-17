using System.ComponentModel;
using System.Reflection;
using BepInEx.Logging;
using DLCLib;
using DLCLib.DLC;
using DLCLib.Screens;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using HarmonyLib;
using HUD;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(DLCPack))]
    [HarmonyPatch("Purchase")]
    public static class DLCPackPurchasePatch
    {
        private static ManualLogSource _log;
        private static LocationChecker _locationChecker;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
        }

        static bool Prefix(DLCPack __instance)
        {
            if (_log == null || _locationChecker == null || __instance == null)
            {
                return true; // run original logic
            }

            _log.LogInfo($"Purchased a DLC! [{__instance.Data.DisplayName}]");
            var stateProperty = typeof(DLCPack).GetProperty("State");
            stateProperty.SetValue(__instance, DLCPackStateEnum.Purchased);
            _locationChecker.AddCheckedLocation(__instance.Data.DisplayName);

            if (__instance.Data.IsBossDLC)
            {
                return true; // run original logic
            }

            return false; // don't run original logic
        }

        static void Postfix(DLCPack __instance)
        {
            InventoryCoinsGetPatch.UpdateCoinsUI();
        }
    }
}
