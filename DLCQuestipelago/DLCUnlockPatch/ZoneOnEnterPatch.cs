using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(Zone))]
    [HarmonyPatch(nameof(Zone.OnEnter))]
    public static class ZoneOnEnterPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
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
                _logger.LogError($"Failed in {nameof(ZoneOnEnterPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
