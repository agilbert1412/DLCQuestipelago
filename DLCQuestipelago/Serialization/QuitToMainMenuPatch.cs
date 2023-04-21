using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Screens;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(MainMenuScreen))]
    [HarmonyPatch("QuitToCampaignScreen")]
    public static class QuitToMainMenuPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // protected void QuitToCampaignScreen()
        private static void Postfix(MainMenuScreen __instance)
        {
            try
            {
                Plugin.Instance.SaveAndQuit();
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(QuitToMainMenuPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
