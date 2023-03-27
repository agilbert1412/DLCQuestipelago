using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("SaveGameData")]
    public static class SaveGameDataPatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        private static void Postfix(DLCSaveManager __instance, ref bool __result)
        {
            DLCQuestipelagoMod.Instance.SaveGame();
        }
    }
}
