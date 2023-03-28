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
            Plugin.Instance.SaveAndQuit();
        }
    }
}
