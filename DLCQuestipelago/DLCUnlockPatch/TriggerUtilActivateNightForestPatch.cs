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
            DLCManager.Instance.UnlockPack("nightmap");
        }
    }
}
