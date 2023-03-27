using DLCLib.Screens;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(MainMenuScreen))]
    [HarmonyPatch("QuitToCampaignScreen")]
    public static class QuitToMainMenuPatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        // protected void QuitToCampaignScreen()
        private static void Postfix(MainMenuScreen __instance)
        {
            DLCQuestipelagoMod.Instance.SaveAndQuit();
        }
    }
}
