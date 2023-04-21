using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch(nameof(TriggerUtil.ActivateForest))]
    public static class TriggerUtilActivateForestPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(TriggerUtilActivateForestPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
