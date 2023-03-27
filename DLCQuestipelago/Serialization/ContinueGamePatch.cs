using DLCLib.Screens;
using GameStateManagement;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(MainMenuScreen))]
    [HarmonyPatch("continueGameEntry_Selected")]
    public static class ContinueGamePatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        // private void continueGameEntry_Selected(object sender, PlayerIndexEventArgs e)
        private static void Postfix(MainMenuScreen __instance, object sender, PlayerIndexEventArgs e)
        {
            if (DLCQuestipelagoMod.Instance.IsInGame)
            {
                return;
            }

            DLCQuestipelagoMod.Instance.EnterGame();
        }
    }
}
