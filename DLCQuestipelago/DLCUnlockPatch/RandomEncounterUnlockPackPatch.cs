using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.Scripts;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(RandomEncounter))]
    [HarmonyPatch(nameof(RandomEncounter.StartRandomEncounterConversation))]
    public static class RandomEncounterUnlockPackPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // public static void StartRandomEncounterConversation()
        static void Postfix()
        {
            DLCManager.Instance.UnlockPack("psychological");
        }
    }
}
