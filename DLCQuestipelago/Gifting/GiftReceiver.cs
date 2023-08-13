using System;
using System.Collections.Generic;
using System.Linq;
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
        private HashSet<Guid> _processedGifts;

        public GiftReceiver(ManualLogSource log, IGiftingService giftService)
        {
            _log = log;
            _giftService = giftService;
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

            return false;
        }

        private bool TryProcessZombieSheep(Gift gift)
        {
            if (gift.Item.Name.Equals("Zombie Sheep", StringComparison.InvariantCultureIgnoreCase))
            {
                _log.LogInfo($"Processing a new Zombie Sheep Gift [ID: {gift.ID}]");
                ProcessZombieSheep(gift.Item.Amount);
                return true;
            }

            return false;
        }

        private void ProcessZombieSheep(int amount)
        {
            for (var i = 1; i <= amount; i++)
            {
                SceneManager.Instance.CurrentScene.Interpolators.Create(0.0f, 1f, i * 2, null, (_) => ItemParser.SpawnZombieSheepOnPlayer());
            }
        }
    }
}
