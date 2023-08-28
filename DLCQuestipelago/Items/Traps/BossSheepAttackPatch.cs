
using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Character;
using DLCQuestipelago.Archipelago;
using HarmonyLib;

namespace DLCQuestipelago.Items.Traps
{
    [HarmonyPatch(typeof(ShopkeepBossNPC))]
    [HarmonyPatch("PerformAttack")]
    internal class BossSheepAttackPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static Random _random;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
            _random = new Random(int.Parse(archipelago.SlotData.Seed));
        }

        //protected override void PerformAttack()
        private static bool Prefix(ShopkeepBossNPC __instance)
        {
            try
            {
                var numberOfSheepReceived = _archipelago.GetReceivedItemCount(TrapManager.ZOMBIE_SHEEP);
                var sheepChance = (double)numberOfSheepReceived / 300.0;

                if (_random.NextDouble() < sheepChance)
                {
                    TrapManager.SpawnZombieSheepOnPlayer();
                    return false; // don't run original logic
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(BossSheepAttackPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
