using System.Collections.Generic;
using Archipelago.Gifting.Net.Gifts.Versions.Current;
using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Archipelago.MultiClient.Net;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Extensions;
using DLCQuestipelago.Items;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Gifting
{
    public class GiftHandler
    {
        private static readonly string[] parsableTraits = new[]
        {
            "Zombie", "Sheep", GiftFlag.Animal, GiftFlag.Monster, GiftFlag.Trap, GiftFlag.Speed
        };

        private readonly ILogger _logger;
        private readonly ArchipelagoSession _session;
        private readonly GiftingService _giftService;
        private readonly GiftSender _giftSender;
        private readonly GiftReceiver _giftReceiver;

        public GiftSender Sender => _giftSender;

        public GiftHandler(ILogger logger, DLCQArchipelagoClient archipelago, ArchipelagoNotificationsHandler notificationHandler, TrapManager trapManager, SpeedChanger speedChanger)
        {
            _logger = logger;
            _session = archipelago.GetSession();
            _giftService = new GiftingService(_session);
            _giftSender = new GiftSender(_logger, archipelago, _giftService);
            _giftReceiver = new GiftReceiver(_logger, _giftService, notificationHandler, trapManager, speedChanger);
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
