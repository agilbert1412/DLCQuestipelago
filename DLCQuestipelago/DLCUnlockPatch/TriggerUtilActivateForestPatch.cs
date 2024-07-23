using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch(nameof(TriggerUtil.ActivateForest))]
    public static class TriggerUtilActivateForestPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public static void ActivateForest(TriggerVolume volume)
        private static void Postfix(TriggerVolume volume)
        {
            try
            {
                DLCManager.Instance.UnlockPack("map");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TriggerUtilActivateForestPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
