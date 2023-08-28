using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Archipelago.Gifting.Net;
using BepInEx.Logging;
using DLCLib;
using DLCQuestipelago.Items;

namespace DLCQuestipelago.Gifting
{
    public class GiftReceiver
    {
        private readonly ManualLogSource _log;
        private IGiftingService _giftService;
        private readonly ArchipelagoNotificationsHandler _notificationHandler;
        private readonly TrapManager _trapManager;
        private readonly SpeedChanger _speedChanger;
        private HashSet<Guid> _processedGifts;

        public GiftReceiver(ManualLogSource log, IGiftingService giftService, ArchipelagoNotificationsHandler notificationHandler, TrapManager trapManager, SpeedChanger speedChanger)
        {
            _log = log;
            _giftService = giftService;
            _notificationHandler = notificationHandler;
            _trapManager = trapManager;
            _speedChanger = speedChanger;
            _processedGifts = new HashSet<Guid>();
        }

        public async Task ReceiveNewGifts()
        {
            var allGifts = await _giftService.CheckGiftBoxAsync();
            ReceiveNewGifts(allGifts);
        }

        public void ReceiveNewGifts(Dictionary<Guid, Gift> gifts)
        {
            if (!gifts.Any())
            {
                return;
            }

            foreach (var giftEntry in gifts)
            {
                var id = giftEntry.Key;
                if (_processedGifts.Contains(id))
                {
                    continue;
                }

                _processedGifts.Add(id);
                var gift = giftEntry.Value; 
                if (!TryProcessGift(gift))
                {
                    _giftService.RefundGift(gift);
                }
            }

            _giftService.RemoveGiftsFromGiftBox(gifts.Keys);
        }

        private bool TryProcessGift(Gift gift)
        {
            if (TryProcessZombieSheep(gift))
            {
                return true;
            }

            if (TryProcessSpeedBoost(gift))
            {
                return true;
            }

            if (TryProcessRandomTrap(gift))
            {
                return true;
            }

            return false;
        }

        private bool TryProcessZombieSheep(Gift gift)
        {
            if (!GiftIsZombieSheep(gift))
            {
                return false;
            }

            _log.LogInfo($"Processing a new Zombie Sheep Gift [ID: {gift.ID}]");
            ProcessZombieSheep(gift.Item.Amount);
            _notificationHandler.AddGiftNotification(gift.Item.Name, true);
            return true;

        }

        private bool TryProcessRandomTrap(Gift gift)
        {
            if (!GiftIsTrap(gift))
            {
                return false;
            }

            _log.LogInfo($"Processing a new Random Trap Gift [ID: {gift.ID}]");
            ProcessRandomTrap(gift.Item.Amount);
            _notificationHandler.AddGiftNotification(gift.Item.Name, true);
            return true;

        }

        private static bool GiftIsZombieSheep(Gift gift)
        {
            var isZombieSheep = gift.Item.Name.Equals("Zombie Sheep", StringComparison.InvariantCultureIgnoreCase);
            var isTrap = GiftIsTrap(gift);
            var zombieSheepTraits = new[] { "Zombie", "Sheep", GiftFlag.Animal, GiftFlag.Monster };
            var hasZombieSheepTrait = gift.Traits.Any(x => zombieSheepTraits.Contains(x.Trait));
            return isZombieSheep || (isTrap && hasZombieSheepTrait);
        }

        private static bool GiftIsTrap(Gift gift)
        {
            return gift.Traits.Any(x => x.Trait.Equals(GiftFlag.Trap));
        }

        private void ProcessZombieSheep(int amount)
        {
            for (var i = 1; i <= amount; i++)
            {
                SceneManager.Instance.CurrentScene.Interpolators.Create(0.0f, 1f, i * 4, null, (_) => TrapManager.SpawnZombieSheepOnPlayer());
            }
        }

        private void ProcessRandomTrap(int amount)
        {
            for (var i = 1; i <= amount; i++)
            {
                SceneManager.Instance.CurrentScene.Interpolators.Create(0.0f, 1f, i * 4, null, (_) => _trapManager.SpawnRandomTrapOnPlayer());
            }
        }

        private bool TryProcessSpeedBoost(Gift gift)
        {
            var speedTraits = gift.Traits.Where(x => x.Trait == GiftFlag.Speed);
            if (!speedTraits.Any())
            {
                return false;
            }

            _log.LogInfo($"Processing a new Speed Boost Gift [ID: {gift.ID}]");
            var amount = speedTraits.Sum(speedTrait => (float)(gift.Item.Amount * speedTrait.Quality));
            ProcessSpeedBoost(amount);
            _notificationHandler.AddGiftNotification(gift.Item.Name, false);
            return true;

        }

        private void ProcessSpeedBoost(float amount)
        {
            const float amountPerBoost = 0.01f;
            var totalBoost = amountPerBoost * amount;
            var multiplier = 1 + totalBoost;
            _speedChanger.AddMultiplierToPlayerSpeed(multiplier);
        }
    }
}
