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
        
        static bool Prefix(DLCManager __instance, string name, bool defaultIfNotFound, ref bool __result)
        {
            __result = false;
            if (Plugin.Instance.HasEnteredGame)
            {
                __result = _itemParser.ReceivedDLCs.Contains(name);
            }

            return false;
        }
    }
}
