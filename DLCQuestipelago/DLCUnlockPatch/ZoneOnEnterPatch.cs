using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

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
        private static void Postfix(Zone __instance, bool isFirstZone)
        {
            try
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
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(ZoneOnEnterPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
