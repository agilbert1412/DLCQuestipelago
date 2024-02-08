using System;
using System.Diagnostics;
using System.Linq;
using BepInEx.Logging;
using DLCLib;
using DLCLib.Campaigns;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch(nameof(Inventory.AddCoin))]
    public static class CoinPickupPatch
    {
        private const string BASIC_CAMPAIGN_NAME = "DLC Quest:";
        private const string LFOD_CAMPAIGN_NAME = "Live Freemium or Die:";
        private const string COIN_LOCATION_NAME = "Coin";

        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        private static bool _wasCollected = false;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _log = log;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void AddCoin()
        private static void Postfix(Inventory __instance)
        {
            try
            {
                CheckAllCoinsanityLocations(__instance);
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(CoinPickupPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }

        public static void CheckAllCoinsanityLocations(Inventory inventory)
        {
            if (_archipelago.SlotData.Coinsanity == Coinsanity.None)
            {
                return; // Let things happened as they have
            }

            var bundleSize = _archipelago.SlotData.GetRealCoinBundleSize();
            var maxCoins = SceneManager.Instance.CurrentScene.Map.TotalCoins;
            var currentCoins = inventory.TotalCoinsCollected;
            var campaignLocation = CampaignManager.Instance.Campaign is LFODCampaign ? LFOD_CAMPAIGN_NAME : BASIC_CAMPAIGN_NAME;

            var checkedCoinLocations = GetAllCheckedCoinLocations(currentCoins, bundleSize, maxCoins, campaignLocation);

            _locationChecker.AddCheckedLocation(checkedCoinLocations);
        }

        public static string[] GetAllCheckedCoinLocations(int totalCoinsPickedUp, double bundleSize, int maxCoins, string campaignName)
        {
            if (totalCoinsPickedUp <= 0)
            {
                return new string[0];
            }

            if (bundleSize < 1)
            {
                return Enumerable.Range(1, totalCoinsPickedUp * 10)
                    .Where(x => x <= maxCoins * 10)
                    .Select(x => $"{campaignName} {x} Coin Piece")
                    .ToArray();
            }

            var checkedCoinLocations = Enumerable.Range(1, totalCoinsPickedUp)
                .Where(x => x <= maxCoins && (x == maxCoins || x % bundleSize == 0))
                .Select(x => $"{campaignName} {x} Coin")
                .ToArray();

            return checkedCoinLocations;
        }
    }
}
