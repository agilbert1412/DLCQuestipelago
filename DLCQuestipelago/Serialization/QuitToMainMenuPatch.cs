using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.Screens;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(MainMenuScreen))]
    [HarmonyPatch("QuitToCampaignScreen")]
    public static class QuitToMainMenuPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
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
                _logger.LogError($"Failed in {nameof(QuitToMainMenuPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
