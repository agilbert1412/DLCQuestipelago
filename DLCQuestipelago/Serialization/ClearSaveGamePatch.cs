using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("ClearSaveGameData")]
    public static class ClearSaveGamePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        private static void Postfix(DLCSaveManager __instance, ref bool __result)
        {
            try
            {
                Plugin.Instance.ExitGame();
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(ClearSaveGamePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
