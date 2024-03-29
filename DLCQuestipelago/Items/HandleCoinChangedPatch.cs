using System;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.HUD;
using DLCQuestipelago.Archipelago;
using HarmonyLib;
using HUD;

namespace DLCQuestipelago.Items
{
    [HarmonyPatch(typeof(CoinDisplay))]
    [HarmonyPatch(nameof(CoinDisplay.HandleCoinChanged))]
    public static class HandleCoinChangedPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
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

                // protected HUDText coinCount;
                var coinCountField = typeof(CoinDisplay).GetField("coinCount", BindingFlags.Instance | BindingFlags.NonPublic);
                var coinCount = (HUDText)coinCountField.GetValue(__instance);

                coinCount.ClearString();
                coinCount.Append(currentCoins.ToString());

                return;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(HandleCoinChangedPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
