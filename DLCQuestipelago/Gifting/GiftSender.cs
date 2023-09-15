using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Archipelago.MultiClient.Net.Helpers;
using BepInEx.Logging;
using DLCLib;
using DLCQuestipelago.Archipelago;

namespace DLCQuestipelago.Gifting
{
    public class GiftSender
    {
        private static readonly HashSet<string> _treeTraits = new() { "Tree", GiftFlag.Material, GiftFlag.Resource, GiftFlag.Wood, "Lumber" };
        private static readonly HashSet<string> _rockTraits = new() { "Rock", "Boulder", GiftFlag.Material, GiftFlag.Resource, GiftFlag.Stone, GiftFlag.Ore };
        private static readonly HashSet<string> _vineTraits = new() { "Vine", GiftFlag.Material, GiftFlag.Resource, GiftFlag.Grass, "Fiber" };
        private static readonly HashSet<string> _zombieSheepTraits = new() { "Zombie", "Sheep", GiftFlag.Animal, GiftFlag.Monster, GiftFlag.Trap };

        private readonly ManualLogSource _log;
        private ArchipelagoClient _archipelago;
        private IGiftingService _giftService;

        public GiftSender(ManualLogSource log, ArchipelagoClient archipelago, IGiftingService giftService)
        {
            _log = log;
            _archipelago = archipelago;
            _giftService = giftService;
        }

        public async Task SendTreeGift(int x, int y)
        {
            _log.LogInfo($"Trying to send a Tree Gift");
            var stringTraits = new HashSet<string>(_treeTraits);
            var validTargetPlayers = await GetPlayersThatCanReceive(stringTraits);
            if (!validTargetPlayers.Any())
            {
                _log.LogInfo($"No player found that can receive a Tree Gift");
                return;
            }
            var giftItem = new GiftItem("Tree", 1, 0);
            var traits = stringTraits.Select(x => new GiftTrait(x, 1, 1)).ToArray();

            await ChooseTargetAndSendGift(validTargetPlayers, giftItem, traits);
        }

        public async Task SendRockGift(int x, int y)
        {
            _log.LogInfo($"Trying to send a Rock Gift");
            var stringTraits = new HashSet<string>(_rockTraits);
            var validTargetPlayers = await GetPlayersThatCanReceive(stringTraits);
            if (!validTargetPlayers.Any())
            {
                _log.LogInfo($"No player found that can receive a Rock Gift");
                return;
            }
            var giftItem = new GiftItem("Rock", 1, 0);
            var traits = stringTraits.Select(x => new GiftTrait(x, 1, 1)).ToArray();

            await ChooseTargetAndSendGift(validTargetPlayers, giftItem, traits);
        }

        public async Task SendVineGift(int x, int y)
        {
            _log.LogInfo($"Trying to send a Vine Gift");
            var stringTraits = new HashSet<string>(_vineTraits);
            var validTargetPlayers = await GetPlayersThatCanReceive(stringTraits);
            if (!validTargetPlayers.Any())
            {
                _log.LogInfo($"No player found that can receive a Vine Gift");
                return;
            }
            var giftItem = new GiftItem("Vine", 1, 0);
            var traits = stringTraits.Select(x => new GiftTrait(x, 1, 1)).ToArray();

            await ChooseTargetAndSendGift(validTargetPlayers, giftItem, traits);
        }

        public async Task SendZombieSheepGift(int x, int y)
        {
            _log.LogInfo($"Trying to send a Zombie Sheep Trap");
            var stringTraits = new HashSet<string>(_zombieSheepTraits);
            var validTargetPlayers = await GetPlayersThatCanReceive(stringTraits);
            if (!validTargetPlayers.Any())
            {
                _log.LogInfo($"No player found that can receive a Zombie Sheep Trap");
                return;
            }
            var giftItem = new GiftItem("Zombie Sheep", 1, 0);
            var traits = stringTraits.Select(x => new GiftTrait(x, 1, 1)).ToArray();

            await ChooseTargetAndSendGift(validTargetPlayers, giftItem, traits);
        }

        private async Task ChooseTargetAndSendGift(List<PlayerInfo> validTargetPlayers, GiftItem giftItem, GiftTrait[] traits)
        {
            var random = new Random(int.Parse(_archipelago.SlotData.Seed) +
                                    (int)StopwatchComponent.Instance.GetElapsedTime().TotalMilliseconds);
            var targetIndex = random.Next(0, validTargetPlayers.Count);
            var target = validTargetPlayers[targetIndex];
            _log.LogInfo($"Target: {target.Name}");
            await SendGiftAsync(giftItem, traits, target);
        }

        private async Task<List<PlayerInfo>> GetPlayersThatCanReceive(HashSet<string> traits)
        {
            var myTeam = _archipelago.Session.ConnectionInfo.Team;
            var playersOtherTeams = _archipelago.Session.Players.Players.Keys.Where(team => team != myTeam);
            var isTrap = traits.Contains(GiftFlag.Trap);
            if (isTrap)
            {
                return await GetPlayersThatCanReceiveTrap(traits, playersOtherTeams, myTeam);
            }

            return await GetPlayersThatCanReceiveGift(traits, myTeam, playersOtherTeams);
        }

        private async Task<List<PlayerInfo>> GetPlayersThatCanReceiveTrap(HashSet<string> traits, IEnumerable<int> playersOtherTeams, int myTeam)
        {
            if (Plugin.Instance.APConnectionInfo.GiftingPreference.HasFlag(GiftingMode.TrapsToEnemies))
            {
                var enemyTargets = await GetEnemyGiftTargets(traits, playersOtherTeams);
                if (enemyTargets.Any())
                {
                    return enemyTargets;
                }
            }

            if (Plugin.Instance.APConnectionInfo.GiftingPreference.HasFlag(GiftingMode.TrapsToAllies))
            {
                var friendlyTargets = await GetFriendlyGiftTargets(traits, myTeam);
                return friendlyTargets;
            }

            return new List<PlayerInfo>();
        }

        private async Task<List<PlayerInfo>> GetPlayersThatCanReceiveGift(HashSet<string> traits, int myTeam, IEnumerable<int> playersOtherTeams)
        {
            if (Plugin.Instance.APConnectionInfo.GiftingPreference.HasFlag(GiftingMode.GiftsToAllies))
            {
                var friendlyTargets = await GetFriendlyGiftTargets(traits, myTeam);
                if (friendlyTargets.Any())
                {
                    return friendlyTargets;
                }
            }

            if (Plugin.Instance.APConnectionInfo.GiftingPreference.HasFlag(GiftingMode.GiftsToEnemies))
            {
                var enemyTargets = await GetEnemyGiftTargets(traits, playersOtherTeams);
                if (enemyTargets.Any())
                {
                    traits.Add(GiftFlag.Trap);
                    return enemyTargets;
                }
            }

            return new List<PlayerInfo>();
        }

        private async Task<List<PlayerInfo>> GetEnemyGiftTargets(HashSet<string> traits, IEnumerable<int> playersOtherTeams)
        {
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

            return validTargets;
        }

        private async Task<List<PlayerInfo>> GetFriendlyGiftTargets(HashSet<string> traits, int myTeam)
        {
            var friendlyTargets = new List<PlayerInfo>();
            foreach (var player in _archipelago.Session.Players.Players[myTeam])
            {
                var canGift = player.Slot != _archipelago.Session.ConnectionInfo.Slot;
                canGift = canGift && await _giftService.CanGiftToPlayerAsync(player.Slot, myTeam, traits);
                if (canGift)
                {
                    friendlyTargets.Add(player);
                }
            }

            return friendlyTargets;
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
