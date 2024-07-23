
using System;
using System.Diagnostics;
using DLCLib.Character;
using DLCQuestipelago.Archipelago;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Items.Traps
{
    [HarmonyPatch(typeof(ShopkeepBossNPC))]
    [HarmonyPatch("PerformAttack")]
    internal class BossSheepAttackPatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;
        private static Random _random;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _random = new Random(int.Parse(archipelago.SlotData.Seed));
        }

        //protected override void PerformAttack()
        private static bool Prefix(ShopkeepBossNPC __instance)
        {
            try
            {
                var numberOfSheepReceived = _archipelago.GetReceivedItemCount(TrapManager.ZOMBIE_SHEEP);
                numberOfSheepReceived = Math.Min(25, numberOfSheepReceived);
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
                _logger.LogError($"Failed in {nameof(BossSheepAttackPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
