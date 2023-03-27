using DLCLib.DLC;
using DLCQuestipelago.Items;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(DLCManager))]
    [HarmonyPatch(nameof(DLCManager.IsPurchased))]
    public static class DLCIsPurchasedPatch
    {
        private static Logger _log;
        private static ItemParser _itemParser;

        public static void Initialize(Logger log, ItemParser itemParser)
        {
            _log = log;
            _itemParser = itemParser;
        }

        private static bool Prefix(DLCManager __instance, string name, bool defaultIfNotFound, ref bool __result)
        {
            __result = defaultIfNotFound;
            __instance.Packs.TryGetValue(name, out var pack);
            if (pack == null)
            {
                return false;
            }

            if (DLCQuestipelagoMod.Instance.IsInGame)
            {
                __result = _itemParser.ReceivedDLCs.Contains(name);
            }

            return false;
        }
    }
}
