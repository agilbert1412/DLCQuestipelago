using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.DLC;
using DLCQuestipelago.Items;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(DLCManager))]
    [HarmonyPatch(nameof(DLCManager.IsPurchased))]
    public static class DLCIsPurchasedPatch
    {
        private static ManualLogSource _log;
        private static ItemParser _itemParser;

        public static void Initialize(ManualLogSource log, ItemParser itemParser)
        {
            _log = log;
            _itemParser = itemParser;
        }

        private static bool Prefix(DLCManager __instance, string name, bool defaultIfNotFound, ref bool __result)
        {
            try
            {
                __result = defaultIfNotFound;
                __instance.Packs.TryGetValue(name, out var pack);
                if (pack == null)
                {
                    return false;
                }

                if (Plugin.Instance.IsInGame)
                {
                    __result = _itemParser.ReceivedDLCs.Contains(name);
                }

                return false;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(DLCIsPurchasedPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
