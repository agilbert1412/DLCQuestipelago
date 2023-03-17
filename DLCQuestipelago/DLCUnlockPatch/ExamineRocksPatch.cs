using BepInEx.Logging;
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
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        //public static bool ExamineRocks(TriggerVolume volume)
        private static void Postfix(TriggerVolume volume)
        {
            var addEventMethod = typeof(Scene).GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            addEventMethod.Invoke(SceneManager.Instance.CurrentScene,
                new object[] { TriggerUtil.ROCKS_DISCOVERED_STR });
        }
    }
}
