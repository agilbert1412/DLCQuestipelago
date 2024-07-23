using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch(nameof(TriggerUtil.EndGame))]
    public static class TriggerUtilGameEndingPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public static void EndGame(TriggerVolume volume)
        private static void Postfix(TriggerVolume volume)
        {
            try
            {
                DLCManager.Instance.UnlockPack("realending");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TriggerUtilGameEndingPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
