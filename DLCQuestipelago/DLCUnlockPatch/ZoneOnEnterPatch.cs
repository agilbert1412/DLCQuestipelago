using BepInEx.Logging;
using DLCLib.Character;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(Zone))]
    [HarmonyPatch(nameof(Zone.OnEnter))]
    public static class ZoneOnEnterPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // public void OnEnter(bool isFirstZone)
        static void Postfix(Zone __instance, bool isFirstZone)
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
