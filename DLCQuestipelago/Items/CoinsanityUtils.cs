using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core;
using DLCLib;
using DLCLib.Campaigns;
using DLCLib.DLC;
using DLCQuestipelago.Archipelago;

namespace DLCQuestipelago.Items
{
    public static class CoinsanityUtils
    {
        public const string BASIC_CAMPAIGN = "DLC Quest";
        public const string LFOD_CAMPAIGN = "Live Freemium or Die";
        public const string AP_COIN_BUNDLE = "Coin Bundle";
        public const string AP_COIN_PIECE = "Coin Piece";
        public const string BASIC_CAMPAIGN_COIN_NAME = $"{BASIC_CAMPAIGN}: {AP_COIN_BUNDLE}";
        public const string LFOD_CAMPAIGN_COIN_NAME = $"{LFOD_CAMPAIGN}: {AP_COIN_BUNDLE}";
        public const string BASIC_CAMPAIGN_COIN_PIECE_NAME = $"{BASIC_CAMPAIGN}: {AP_COIN_PIECE}";
        public const string LFOD_CAMPAIGN_COIN_PIECE_NAME = $"{LFOD_CAMPAIGN}: {AP_COIN_PIECE}";

        private static string GetRelevantCoinName(ArchipelagoClient archipelago)
        {
            return CampaignManager.Instance.Campaign is DLCQuestCampaign
                ? (archipelago.SlotData.CoinBundleSize > 0 ? BASIC_CAMPAIGN_COIN_NAME : BASIC_CAMPAIGN_COIN_PIECE_NAME)
                : (archipelago.SlotData.CoinBundleSize > 0 ? LFOD_CAMPAIGN_COIN_NAME : LFOD_CAMPAIGN_COIN_PIECE_NAME);
        }

        public static double GetCurrentCoins(ArchipelagoClient archipelago)
        {
            if (archipelago.SlotData.Coinsanity == Coinsanity.None)
            {
                return Singleton<SceneManager>.Instance.CurrentScene.Player.Inventory.Coins;
            }
            var receivedCoinBundles = archipelago.GetReceivedItemCount(GetRelevantCoinName(archipelago));
            var obtainedCoins = receivedCoinBundles * archipelago.SlotData.GetRealCoinBundleSize();

            if (archipelago.SlotData.PermanentCoins)
            {
                return obtainedCoins;
            }

            var spentCoins = DLCManager.Instance.Packs.Values.Where(x => x.State == DLCPackStateEnum.Purchased)
                .Sum(x => x.Data.Cost);
            var currentCoins = obtainedCoins - spentCoins;
            return currentCoins;
        }

        public static void UpdateCoinsUI()
        {
            var onCoinChangeMethod = typeof(Inventory).GetMethod("OnCoinChange", BindingFlags.NonPublic | BindingFlags.Instance);
            onCoinChangeMethod.Invoke(SceneManager.Instance.CurrentScene.Player.Inventory, new object[0]);
        }
    }
}
