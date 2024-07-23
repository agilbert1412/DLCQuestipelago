using System;
using System.Diagnostics;
using DLCLib.DLC;
using DLCQuestipelago.Items;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Shop
{
    [HarmonyPatch(typeof(DLCManager))]
    [HarmonyPatch(nameof(DLCManager.IsPurchased))]
    public static class DLCIsPurchasedPatch
    {
        private static ILogger _logger;
        private static ItemParser _itemParser;

        public static void Initialize(ILogger logger, ItemParser itemParser)
        {
            _logger = logger;
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
                _logger.LogError($"Failed in {nameof(DLCIsPurchasedPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
