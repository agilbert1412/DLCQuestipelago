using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch(nameof(TriggerUtil.EndGame))]
    public static class TriggerUtilGameEndingPatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        // public static void EndGame(TriggerVolume volume)
        private static void Postfix(TriggerVolume volume)
        {
            DLCManager.Instance.UnlockPack("realending");
        }
    }
}
