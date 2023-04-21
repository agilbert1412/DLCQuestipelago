using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("SaveGameData")]
    public static class SaveGameDataPatch
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
                Plugin.Instance.SaveGame();
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(SaveGameDataPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
