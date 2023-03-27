using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("ClearSaveGameData")]
    public static class ClearSaveGamePatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        private static void Postfix(DLCSaveManager __instance, ref bool __result)
        {
            DLCQuestipelagoMod.Instance.ExitGame();
        }
    }
}
