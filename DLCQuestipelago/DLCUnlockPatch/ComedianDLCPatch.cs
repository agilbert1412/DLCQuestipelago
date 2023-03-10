using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using DLCLib.Character;
using DLCLib.DLC;
using DLCLib.World;
using HarmonyLib;
using Microsoft.Xna.Framework;

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
        static void Postfix(ComedianNPC __instance, ref bool __result)
        {
            if (!__instance.IsAlive)
            {
                var spawnDlcPackMethod = typeof(ComedianNPC).GetMethod("SpawnDLCPack", BindingFlags.NonPublic | BindingFlags.Instance);
                spawnDlcPackMethod.Invoke(__instance, new object[0]);
            }
        }
    }
}
