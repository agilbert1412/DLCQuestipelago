using System;
using System.Diagnostics;
using DLCLib.Screens;
using DLCQuestipelago.Archipelago;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(CampaignSelectScreen))]
    [HarmonyPatch("StartCampaign")]
    public static class CampaignSelectPatch
    {
        private const string BASIC_CAMPAIGN = "dlcquest";
        private const string LFOD_CAMPAIGN = "lfod";

        private static ILogger _logger;
        private static SlotData _slotData;

        public static void Initialize(ILogger logger, SlotData slotData)
        {
            _logger = logger;
            _slotData = slotData;
        }

        // private void StartCampaign(string selectedCampaignName)
        private static bool Prefix(CampaignSelectScreen __instance, string selectedCampaignName)
        {
            try
            {
                if (_slotData.Campaign == Campaign.Basic && selectedCampaignName == LFOD_CAMPAIGN)
                {
                    return false; // don't run original logic
                }

                if (_slotData.Campaign == Campaign.LiveFreemiumOrDie && selectedCampaignName == BASIC_CAMPAIGN)
                {
                    return false; // don't run original logic
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CampaignSelectPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
