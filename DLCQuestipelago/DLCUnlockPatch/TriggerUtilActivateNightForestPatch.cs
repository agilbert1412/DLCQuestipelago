using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch(nameof(TriggerUtil.ActivateNightForest))]
    public static class TriggerUtilActivateNightForestPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // public static void ActivateNightForest(TriggerVolume volume)
        private static void Postfix(TriggerVolume volume)
        {
            try
            {
                DLCManager.Instance.UnlockPack("nightmap");
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(TriggerUtilActivateNightForestPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
