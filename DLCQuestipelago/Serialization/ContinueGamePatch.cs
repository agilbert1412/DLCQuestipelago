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
            if (Plugin.Instance.HasEnteredGame)
            {
                return;
            }
            
            if (File.Exists(Persistency.SaveFile))
            {
                var fileContent = File.ReadAllText(Persistency.SaveFile);
                Plugin.Instance.ArchipelagoState = JsonConvert.DeserializeObject<ArchipelagoStateDto>(fileContent);
            }
            else
            {
                Plugin.Instance.ArchipelagoState = new ArchipelagoStateDto();
            }
            Plugin.Instance.EnterGame();
        }
    }
}
