using System.IO;
using BepInEx.Logging;
using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("StartNewGame")]
    public static class StartNewGamePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        static void Postfix(DLCSaveManager __instance)
        {
            if (File.Exists(Persistency.SaveFile))
            {
                File.Delete(Persistency.SaveFile);
            }
            Plugin.Instance.ArchipelagoState = new ArchipelagoStateDto();
            Plugin.Instance.EnterGame();
        }
    }
}
