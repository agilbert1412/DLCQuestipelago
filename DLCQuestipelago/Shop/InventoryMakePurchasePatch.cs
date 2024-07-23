﻿using System;
using System.Diagnostics;
using DLCLib;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Items;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Shop
{
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.MakePurchase))]
    public static class InventoryMakePurchasePatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public bool MakePurchase(int purchaseAmount, bool useBossCoins)
        private static bool Prefix(Inventory __instance, int purchaseAmount, bool useBossCoins, ref bool __result)
        {
            try
            {
                if (useBossCoins && __instance.BossCoins >= purchaseAmount)
                {
                    __instance.BossCoins -= purchaseAmount;
                    __instance.OnBossCoinChange();
                    __result = true;
                    return false; // don't run original logic
                }
                if (!useBossCoins && __instance.Coins >= purchaseAmount)
                {
                    if (!_archipelago.SlotData.PermanentCoins)
                    {
                        __instance.Coins -= purchaseAmount;
                    }
                    CoinsanityUtils.UpdateCoinsUI();
                    __result = true;
                    return false; // don't run original logic
                }

                __result = false;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(InventoryMakePurchasePatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
