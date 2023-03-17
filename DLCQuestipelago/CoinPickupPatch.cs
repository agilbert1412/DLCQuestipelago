using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.Audio;
using DLCLib.Campaigns;
using DLCLib.Physics;
using DLCLib.Screens;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;
using HUD;
using Microsoft.Xna.Framework;
using System.Reflection;

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
        static void Postfix(Inventory __instance)
        {
            if (_archipelago.SlotData.Coinsanity == Coinsanity.None)
            {
                return; // Let things happened as they have
            }

            var campaignLocation = CampaignManager.Instance.Campaign is LFODCampaign ? LFOD_CAMPAIGN_NAME : BASIC_CAMPAIGN_NAME;
            var bundleSize = _archipelago.SlotData.CoinBundleSize;
            for (var i = bundleSize; i <= __instance.TotalCoinsCollected; i += bundleSize)
            {
                var location = $"{campaignLocation} {i} {COIN_LOCATION_NAME}";
                _log.LogDebug($"Checking Coin Bundle Location: {location}");
                _locationChecker.AddCheckedLocation(location);
            }
        }
    }
}
