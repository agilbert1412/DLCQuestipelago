using BepInEx.Logging;
using DLCLib.DLC;
using DLCLib.World.Props;
using HarmonyLib;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(Grindstone))]
    [HarmonyPatch(nameof(Grindstone.Activate))]
    public static class GrindstoneUnlockPackPatch
    {
        private static ManualLogSource _log;
        private static int grindCount;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
            grindCount = 0;
        }

        // public bool Activate()
        private static void Postfix(Grindstone __instance, ref bool __result)
        {
            grindCount++;
            if (grindCount >= 9)
            {
                DLCManager.Instance.UnlockPack("grindstone");
            }
        }
    }
}
