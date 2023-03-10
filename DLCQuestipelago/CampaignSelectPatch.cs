using BepInEx.Logging;
using DLCLib;
using DLCLib.Screens;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(CampaignSelectScreen))]
    [HarmonyPatch("StartCampaign")]
    public static class CampaignSelectPatch
    {
        private const string BASIC_CAMPAIGN = "dlcquest";
        private const string LFOD_CAMPAIGN = "lfod";

        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        // private void StartCampaign(string selectedCampaignName)
        static bool Prefix(CampaignSelectScreen __instance, string selectedCampaignName)
        {
            if (_archipelago.SlotData.Campaign == Campaign.Basic && selectedCampaignName == LFOD_CAMPAIGN)
            {
                return false; // don't run original logic
            }

            if (_archipelago.SlotData.Campaign == Campaign.LiveFreemiumOrDie && selectedCampaignName == BASIC_CAMPAIGN)
            {
                return false; // don't run original logic
            }

            return true; // run original logic
        }
    }
}
