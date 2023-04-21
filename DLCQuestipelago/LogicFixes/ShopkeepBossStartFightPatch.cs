using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using DLCLib.Character;
using HarmonyLib;

namespace DLCQuestipelago.LogicFixes
{
    [HarmonyPatch(typeof(ShopkeepBossNPC))]
    [HarmonyPatch(nameof(ShopkeepBossNPC.StartFight))]
    public static class ShopkeepBossStartFightPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(ShopkeepBossStartFightPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
