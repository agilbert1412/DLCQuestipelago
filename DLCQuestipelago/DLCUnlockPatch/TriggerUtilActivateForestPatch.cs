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
            DLCManager.Instance.UnlockPack("map");
        }
    }
}
