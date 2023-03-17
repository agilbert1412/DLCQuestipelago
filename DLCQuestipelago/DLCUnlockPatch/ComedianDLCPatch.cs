using BepInEx.Logging;
using DLCLib.Character;
using HarmonyLib;
using System.Reflection;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(ComedianNPC))]
    [HarmonyPatch(nameof(ComedianNPC.Activate))]
    public static class ComedianDLCPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // public override bool Activate()
        private static void Postfix(ComedianNPC __instance, ref bool __result)
        {
            if (!__instance.IsAlive)
            {
                var spawnDlcPackMethod = typeof(ComedianNPC).GetMethod("SpawnDLCPack", BindingFlags.NonPublic | BindingFlags.Instance);
                spawnDlcPackMethod.Invoke(__instance, new object[0]);
            }
        }
    }
}
