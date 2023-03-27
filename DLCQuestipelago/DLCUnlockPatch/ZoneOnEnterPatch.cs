using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(Zone))]
    [HarmonyPatch(nameof(Zone.OnEnter))]
    public static class ZoneOnEnterPatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        // public void OnEnter(bool isFirstZone)
        private static void Postfix(Zone __instance, bool isFirstZone)
        {
            if (!isFirstZone)
            {
                DLCManager.Instance.UnlockPack("loading");
            }
            if (__instance.IsSnowZone)
            {
                DLCManager.Instance.UnlockPack("seasonpass");
            }
        }
    }
}
