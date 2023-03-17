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

        private static void Postfix(DLCSaveManager __instance)
        {
            Plugin.Instance.EnterGame();
        }
    }
}
