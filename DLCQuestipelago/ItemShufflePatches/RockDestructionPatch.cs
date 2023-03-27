using DLCLib;
using DLCLib.World;
using DLCLib.World.Props;
using HarmonyLib;
using System.Reflection;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(Rock))]
    [HarmonyPatch("OnDestruction")]
    public static class RockDestructionPatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        //protected override void OnDestruction()
        private static void Postfix(Rock __instance)
        {
            var addEventMethod = typeof(Scene).GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance);
            addEventMethod.Invoke(SceneManager.Instance.CurrentScene,
                new object[] { TriggerUtil.ROCKS_DISCOVERED_STR });
        }
    }
}
