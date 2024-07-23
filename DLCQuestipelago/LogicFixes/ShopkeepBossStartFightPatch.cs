using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.Character;
using HarmonyLib;

namespace DLCQuestipelago.LogicFixes
{
    [HarmonyPatch(typeof(ShopkeepBossNPC))]
    [HarmonyPatch(nameof(ShopkeepBossNPC.StartFight))]
    public static class ShopkeepBossStartFightPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public void StartFight()
        private static bool Prefix(ShopkeepBossNPC __instance)
        {
            try
            {
                var attackStateField =
                    typeof(ShopkeepBossNPC).GetField("attackState", BindingFlags.NonPublic | BindingFlags.Instance);
                var attackStateEnum = typeof(ShopkeepBossNPC).GetNestedTypes(BindingFlags.NonPublic)
                    .First(x => x.Name == "AttackStateEnum");
                var attackStateSpawning =
                    attackStateEnum.GetField("Spawning", BindingFlags.Static | BindingFlags.Public);
                attackStateField.SetValue(__instance, attackStateSpawning.GetValue(null));
                return true; // Run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ShopkeepBossStartFightPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
