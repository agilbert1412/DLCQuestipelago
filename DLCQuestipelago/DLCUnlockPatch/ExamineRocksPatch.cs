using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib;
using DLCLib.World;
using HarmonyLib;
using System.Reflection;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch(nameof(TriggerUtil.ExamineRocks))]
    public static class ExamineRocksPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        //public static bool ExamineRocks(TriggerVolume volume)
        private static void Postfix(TriggerVolume volume)
        {
            try
            {
                var addEventMethod =
                    typeof(Scene).GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance);
                addEventMethod.Invoke(SceneManager.Instance.CurrentScene,
                    new object[] { TriggerUtil.ROCKS_DISCOVERED_STR });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ExamineRocksPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
