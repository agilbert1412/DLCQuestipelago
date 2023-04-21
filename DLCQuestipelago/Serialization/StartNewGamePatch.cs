using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("StartNewGame")]
    public static class StartNewGamePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        private static void Postfix(DLCSaveManager __instance)
        {
            try
            {
                Plugin.Instance.EnterGame();
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(StartNewGamePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
