using System;
using System.Collections.Generic;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Archipelago.MultiClient.Net;
using BepInEx.Logging;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Extensions;
using DLCQuestipelago.Items;

namespace DLCQuestipelago.Gifting
{
    public class GiftHandler
    {
        private static readonly string[] parsableTraits = new[]
        {
            "Zombie", "Sheep", GiftFlag.Animal, GiftFlag.Monster, GiftFlag.Trap, GiftFlag.Speed
        };

        private readonly ManualLogSource _log;
        private readonly ArchipelagoSession _session;
        private readonly GiftingService _giftService;
        private readonly GiftSender _giftSender;
        private readonly GiftReceiver _giftReceiver;

        public GiftSender Sender => _giftSender;

        public GiftHandler(ManualLogSource log, ArchipelagoClient archipelago, ArchipelagoNotificationsHandler notificationHandler, TrapManager trapManager, SpeedChanger speedChanger)
        {
            _log = log;
            _session = archipelago.Session;
            _giftService = new GiftingService(_session);
            _giftSender = new GiftSender(_log, archipelago, _giftService);
            _giftReceiver = new GiftReceiver(_log, _giftService, notificationHandler, trapManager, speedChanger);
            archipelago.SetGiftHandler(this);
        }

        public void OpenGiftBox()
        {
            _giftService.OpenGiftBox(false, parsableTraits);
            // _giftService.SubscribeToNewGifts(NewGiftNotification);
        }

        public void CloseGiftBox()
        {
            _giftService.CloseGiftBox();
        }

        public void NewGiftNotification(Dictionary<string, Gift> gifts)
        {
            _giftReceiver.ReceiveNewGifts(gifts);
        }

        public void NewGiftNotification()
        {
            _giftReceiver.ReceiveNewGifts().FireAndForget();
        }
    }
}
