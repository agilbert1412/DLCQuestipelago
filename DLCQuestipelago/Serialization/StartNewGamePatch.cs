using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("StartNewGame")]
    public static class StartNewGamePatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        private static void Postfix(DLCSaveManager __instance)
        {
            DLCQuestipelagoMod.Instance.EnterGame();
        }
    }
}
