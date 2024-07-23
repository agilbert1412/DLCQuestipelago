using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("ClearSaveGameData")]
    public static class ClearSaveGamePatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        private static void Postfix(DLCSaveManager __instance, ref bool __result)
        {
            try
            {
                Plugin.Instance.ExitGame();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ClearSaveGamePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
