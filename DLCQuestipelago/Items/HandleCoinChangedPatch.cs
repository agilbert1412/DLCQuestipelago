using System;
using System.Diagnostics;
using System.Reflection;
using Core;
using DLCLib;
using DLCLib.HUD;
using DLCQuestipelago.Archipelago;
using HarmonyLib;
using HUD;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Items
{
    [HarmonyPatch(typeof(CoinDisplay))]
    [HarmonyPatch(nameof(CoinDisplay.HandleCoinChanged))]
    public static class HandleCoinChangedPatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        //public void HandleCoinChanged(int numCoins)
        private static void Postfix(CoinDisplay __instance, int numCoins)
        {
            try
            {
                if (!Plugin.Instance.IsInGame || _archipelago == null || Singleton<SceneManager>.Instance.CurrentScene.IsBossMode)
                {
                    return;
                }

                if (_archipelago.SlotData.Coinsanity == Coinsanity.None || _archipelago.SlotData.CoinBundleSize > 0)
                {
                    return;
                }

                var currentCoins = CoinsanityUtils.GetCurrentCoins(_archipelago);

                currentCoins = Math.Round(currentCoins, 2);

                // protected HUDText coinCount;
                var coinCountField = typeof(CoinDisplay).GetField("coinCount", BindingFlags.Instance | BindingFlags.NonPublic);
                var coinCount = (HUDText)coinCountField.GetValue(__instance);

                coinCount.ClearString();
                coinCount.Append(currentCoins.ToString());

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HandleCoinChangedPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
