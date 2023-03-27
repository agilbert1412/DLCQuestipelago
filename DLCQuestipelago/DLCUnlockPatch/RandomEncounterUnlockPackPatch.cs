using DLCLib.DLC;
using DLCLib.Scripts;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(RandomEncounter))]
    [HarmonyPatch(nameof(RandomEncounter.StartRandomEncounterConversation))]
    public static class RandomEncounterUnlockPackPatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        // public static void StartRandomEncounterConversation()
        private static void Postfix()
        {
            DLCManager.Instance.UnlockPack("psychological");
        }
    }
}
