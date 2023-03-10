using System.Reflection;
using BepInEx.Logging;
using DLCLib;
using DLCLib.World;
using DLCLib.World.Props;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(Rock))]
    [HarmonyPatch("OnDestruction")]
    public static class RockDestructionPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        //protected override void OnDestruction()
        static void Postfix(Rock __instance)
        {
            var addEventMethod = typeof(Scene).GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            addEventMethod.Invoke(SceneManager.Instance.CurrentScene,
                new object[] { TriggerUtil.ROCKS_DISCOVERED_STR });
        }
    }
}
