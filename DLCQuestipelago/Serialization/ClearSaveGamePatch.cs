using BepInEx.Logging;
using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("ClearSaveGameData")]
    public static class ClearSaveGamePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        private static void Postfix(DLCSaveManager __instance, ref bool __result)
        {
            Plugin.Instance.ExitGame();
        }
    }
}
