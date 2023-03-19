using BepInEx.Logging;
using DLCLib;
using DLCLib.Campaigns;
using DLCLib.DLC;
using DLCLib.HUD;
using DLCQuestipelago.Archipelago;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace DLCQuestipelago.Items
{
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.Coins), MethodType.Getter)]
    public static class InventoryCoinsGetPatch
    {
        public const string BASIC_CAMPAIGN = "DLC Quest";
        public const string LFOD_CAMPAIGN = "Live Freemium or Die";
        public const string AP_COIN_BUNDLE = "Coin Bundle";
        public const string BASIC_CAMPAIGN_COIN_NAME = $"{BASIC_CAMPAIGN}: {AP_COIN_BUNDLE}";
        public const string LFOD_CAMPAIGN_COIN_NAME = $"{LFOD_CAMPAIGN}: {AP_COIN_BUNDLE}";

        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        private static CoinDisplay _coinDisplay => SceneManager.Instance.CurrentScene.HUDManager.CoinDisplay;
        private static string _relevantCoinName => CampaignManager.Instance.Campaign is DLCQuestCampaign ? BASIC_CAMPAIGN_COIN_NAME : LFOD_CAMPAIGN_COIN_NAME;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        //public override void OnPickup(Player player)
        private static bool Prefix(Inventory __instance, ref int __result)
        {
            if (!Plugin.Instance.IsInGame || _archipelago == null)
            {
                return true; // run original logic;
            }
            if (_archipelago.SlotData.Coinsanity == Coinsanity.None)
            {
                return true; // run original logic
            }

            var receivedCoinBundles = _archipelago.GetReceivedItemCount(_relevantCoinName);
            var obtainedCoins = receivedCoinBundles * _archipelago.SlotData.CoinBundleSize;
            var spentCoins = DLCManager.Instance.Packs.Values.Where(x => x.State == DLCPackStateEnum.Purchased).Sum(x => x.Data.Cost);
            var currentCoins = obtainedCoins - spentCoins;
            __result = currentCoins + 4;
            // _coinDisplay.HandleCoinChanged(currentCoins);
            return false; // don't run original logic
        }

        public static void UpdateCoinsUI()
        {
            var onCoinChangeMethod = typeof(Inventory).GetMethod("OnCoinChange", BindingFlags.NonPublic | BindingFlags.Instance);
            onCoinChangeMethod.Invoke(SceneManager.Instance.CurrentScene.Player.Inventory, new object[0]);
        }
    }
}
