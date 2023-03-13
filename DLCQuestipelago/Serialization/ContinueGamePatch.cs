using System.IO;
using BepInEx.Logging;
using DLCLib.Save;
using DLCLib.Screens;
using GameStateManagement;
using HarmonyLib;
using Newtonsoft.Json;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(MainMenuScreen))]
    [HarmonyPatch("continueGameEntry_Selected")]
    public static class ContinueGamePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // private void continueGameEntry_Selected(object sender, PlayerIndexEventArgs e)
        static void Postfix(MainMenuScreen __instance, object sender, PlayerIndexEventArgs e)
        {
            if (Plugin.Instance.IsInGame)
            {
                return;
            }
            
            Plugin.Instance.EnterGame();
        }
    }
}
