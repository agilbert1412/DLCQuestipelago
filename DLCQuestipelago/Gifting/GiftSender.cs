using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.Gifting.Net;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;
using BepInEx.Logging;
using DLCLib;
using DLCQuestipelago.Archipelago;

namespace DLCQuestipelago.Gifting
{
    public class GiftSender
    {
        private static readonly string[] _zombieSheepTraits = new[]
            { "Zombie", "Sheep", GiftFlag.Animal, GiftFlag.Monster, GiftFlag.Trap };

        private readonly ManualLogSource _log;
        private ArchipelagoClient _archipelago;
        private GiftingService _giftService;

        public GiftSender(ManualLogSource log, ArchipelagoClient archipelago, GiftingService giftService)
        {
            _log = log;
            _archipelago = archipelago;
            _giftService = giftService;
        }

        public async Task SendZombieSheepGift(int x, int y)
        {
            _log.LogInfo($"Trying to send a Zombie Sheep Gift");
            var validTargetPlayers = await GetPlayersThatCanReceive(_zombieSheepTraits);
            if (!validTargetPlayers.Any())
            {
                return;
            }

            var random = new Random(int.Parse(_archipelago.SlotData.Seed) + (int)StopwatchComponent.Instance.GetElapsedTime().TotalMilliseconds);
            var targetIndex = random.Next(0, validTargetPlayers.Count);
            var target = validTargetPlayers[targetIndex];

            _log.LogInfo($"Target: {target.Name}");
            var giftItem = new GiftItem("Zombie Sheep", 1, 0);
            var traits = _zombieSheepTraits.Select(x => new GiftTrait(x, 1, 1)).ToArray();
            await SendGiftAsync(giftItem, traits, target);
        }

        private async Task<List<PlayerInfo>> GetPlayersThatCanReceive(string[] traits)
        {
            var myTeam = _archipelago.Session.ConnectionInfo.Team;
            var playersOtherTeams = _archipelago.Session.Players.Players.Keys.Where(team => team != myTeam);
            var validTargets = new List<PlayerInfo>();
            foreach (var team in playersOtherTeams)
            {
                foreach (var player in _archipelago.Session.Players.Players[team])
                {
                    var canGift = await _giftService.CanGiftToPlayerAsync(player.Slot, team, traits);
                    if (canGift)
                    {
                        validTargets.Add(player);
                    }
                }
            }

            if (validTargets.Any())
            {
                return validTargets;
            }
            
            foreach (var player in _archipelago.Session.Players.Players[myTeam])
            {
                if (player.Slot != _archipelago.Session.ConnectionInfo.Slot && _giftService.CanGiftToPlayer(player.Slot, myTeam, traits))
                {
                    validTargets.Add(player);
                }
            }

            return validTargets;
        }

        public async Task SendGiftAsync(GiftItem giftItem, GiftTrait[] traits, PlayerInfo target)
        {
            var result = await _giftService.SendGiftAsync(giftItem, traits, target.Name, target.Team);
            if (result)
            {
                _log.LogInfo($"Successfully send a gift to {target.Name}");
            }
            else
            {
                _log.LogWarning($"Failed at sending a trap gift to {target.Name}");
            }
        }

        public void SendGiftSync(GiftItem giftItem, GiftTrait[] traits, PlayerInfo target)
        {
            var result = _giftService.SendGift(giftItem, traits, target.Name, target.Team, out var giftId);
            Debug.Assert(result, $"Failed at sending a trap gift to {target.Name}");
            if (result)
            {
                _log.LogInfo($"Successfully send a gift to {target.Name}");
            }
        }
    }
}
