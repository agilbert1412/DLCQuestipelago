using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.Character;
using HarmonyLib;
using System.Reflection;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(ComedianNPC))]
    [HarmonyPatch(nameof(ComedianNPC.Activate))]
    public static class ComedianDLCPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public override bool Activate()
        private static void Postfix(ComedianNPC __instance, ref bool __result)
        {
            try
            {
                if (!__instance.IsAlive)
                {
                    var spawnDlcPackMethod = typeof(ComedianNPC).GetMethod("SpawnDLCPack",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    spawnDlcPackMethod.Invoke(__instance, new object[0]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ComedianDLCPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
