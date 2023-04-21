using System;
using System.Diagnostics;
using BepInEx.Logging;
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
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        //protected override void OnDestruction()
        private static void Postfix(Rock __instance)
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
                _log.LogError($"Failed in {nameof(RockDestructionPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
