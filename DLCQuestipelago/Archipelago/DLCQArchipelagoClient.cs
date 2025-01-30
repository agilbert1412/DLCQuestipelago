using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DLCQuestipelago.Archipelago.Deathlink;
using DLCQuestipelago.Gifting;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Archipelago
{
    public class DLCQArchipelagoClient : ArchipelagoClient
    {
        public override string GameName => "DLCQuest";
        public override string ModName => "DLCQuestipelago";
        public override string ModVersion => PluginInfo.PLUGIN_VERSION;

        public SlotData SlotData => (SlotData)_slotData;
        private GiftHandler _giftHandler;

        public DLCQArchipelagoClient(ILogger logger, Action itemReceivedFunction) :
            base(logger, new DataPackageCache("dlc_quest", "BepInEx", "plugins", "DLCQuestipelago", "IdTables"), itemReceivedFunction)
        {
        }

        protected override void InitializeSlotData(string slotName, Dictionary<string, object> slotDataFields)
        {
            _slotData = new SlotData(slotName, slotDataFields, Logger);
        }

        protected override void OnMessageReceived(LogMessage message)
        {
            try
            {
                var fullMessage = string.Join(" ", message.Parts.Select(str => str.Text));
                Logger.LogInfo(fullMessage);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed in {nameof(ArchipelagoClient)}.{nameof(OnMessageReceived)}:\n\t{ex}");
                Debugger.Break();
                return; // run original logic
            }
        }

        protected override void KillPlayerDeathLink(DeathLink deathLink)
        {
            DiePatch.ReceiveDeathLink();
        }

        internal void SetGiftHandler(GiftHandler giftHandler)
        {
            _giftHandler = giftHandler;
        }
    }
}
