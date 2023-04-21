using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch(nameof(TriggerUtil.EndGame))]
    public static class TriggerUtilGameEndingPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(TriggerUtilGameEndingPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
