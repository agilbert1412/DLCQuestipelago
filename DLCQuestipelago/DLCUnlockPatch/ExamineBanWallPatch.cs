using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib;
using DLCLib.DLC;
using DLCLib.Scripts.LFOD;
using DLCLib.World;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch(nameof(TriggerUtil.ExamineBanWall))]
    public static class ExamineBanWallPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public static bool ExamineBanWall(TriggerVolume volume)
        private static void Postfix(TriggerVolume volume)
        {
            try
            {
                if (!SceneManager.Instance.CurrentScene.EventList.Contains(FakeEnding.FAKE_ENDING_COMPLETE_STR))
                {
                    return;
                }

                DLCManager.Instance.UnlockPack("namechange");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ExamineBanWallPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
