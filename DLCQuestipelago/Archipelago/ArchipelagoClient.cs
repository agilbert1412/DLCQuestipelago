using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx.Logging;
using DLCQuestipelago.Archipelago.Deathlink;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using DLCLib;
using DLCQuestipelago.Gifting;

namespace DLCQuestipelago.Archipelago
{
    public class ArchipelagoClient
    {
        private const string GAME_NAME = "DLCQuest";
        private ManualLogSource _console;
        private ArchipelagoSession _session;
        private DeathLinkService _deathLinkService;
        private GiftHandler _giftHandler;
        private Harmony _harmony;

        private ArchipelagoConnectionInfo _connectionInfo;

        private Action _itemReceivedFunction;

        public ArchipelagoSession Session => _session;

        public bool IsConnected { get; private set; }
        public SlotData SlotData { get; private set; }
        public Dictionary<string, ScoutedLocation> ScoutedLocations { get; set; }

        private DataPackageCache _localDataPackage;

        public ArchipelagoClient(ManualLogSource console, Harmony harmony, Action itemReceivedFunction)
        {
            _console = console;
            _harmony = harmony;
            _itemReceivedFunction = itemReceivedFunction;
            IsConnected = false;
            ScoutedLocations = new Dictionary<string, ScoutedLocation>();
            _localDataPackage = new DataPackageCache();
        }

        public void Connect(ArchipelagoConnectionInfo connectionInfo, out string errorMessage)
        {
            DisconnectPermanently();
            _connectionInfo = connectionInfo;
            var success = TryConnect(_connectionInfo, out errorMessage);
            if (!success)
            {
                DisconnectPermanently();
                return;
            }
        }

        private bool TryConnect(ArchipelagoConnectionInfo connectionInfo, out string errorMessage)
        {
            LoginResult result;
            try
            {
                InitSession(connectionInfo);
                var itemsHandling = ItemsHandlingFlags.AllItems;
                var minimumVersion = new Version(0, 4, 0);
                var tags = connectionInfo.DeathLink ? new[] { "AP", "DeathLink" } : new[] { "AP" };
                result = _session.TryConnectAndLogin(GAME_NAME, _connectionInfo.SlotName, itemsHandling, minimumVersion, tags, null, _connectionInfo.Password);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
                var failure = (LoginFailure)result;
                errorMessage = $"Failed to Connect to {_connectionInfo.HostUrl}:{_connectionInfo.Port} as {_connectionInfo.SlotName}:";
                foreach (var error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }

                var detailedErrorMessage = errorMessage;
                foreach (var error in failure.ErrorCodes)
                {
                    detailedErrorMessage += $"\n    {error}";
                }

                _console.LogError(detailedErrorMessage);
                DisconnectAndCleanup();
                return false; // Did not connect, show the user the contents of `errorMessage`
            }

            errorMessage = "";

            // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be used to interact with the server and the returned `LoginSuccessful` contains some useful information about the initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
            var loginSuccess = (LoginSuccessful)result;
            var loginMessage = $"Connected to Archipelago server as {connectionInfo.SlotName} (Team {loginSuccess.Team}).";
            _console.LogInfo(loginMessage);

            // Must go AFTER a successful connection attempt
            InitializeSlotData(connectionInfo.SlotName, loginSuccess.SlotData);
            InitializeAfterConnection();
            connectionInfo.DeathLink = SlotData.DeathLink;
            return true;
        }

        private void InitializeAfterConnection()
        {
            if (_session == null)
            {
                _console.LogError($"_session is null in InitializeAfterConnection(). This should NEVER happen");
                DisconnectPermanently();
                return;
            }

            IsConnected = true;

            _session.Items.ItemReceived += OnItemReceived;
            _session.MessageLog.OnMessageReceived += OnMessageReceived;
            _session.Socket.ErrorReceived += SessionErrorReceived;
            _session.Socket.SocketClosed += SessionSocketClosed;

            InitializeDeathLink();
            // MultiRandom = new Random(SlotData.Seed);
        }

        public void Sync()
        {
            if (!MakeSureConnected(0))
            {
                return;
            }

            try
            {
                _session.Socket.SendPacket(new SyncPacket());
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
            }
        }

        private void InitializeDeathLink()
        {
            _deathLinkService = _session.CreateDeathLinkService();
            if (SlotData.DeathLink)
            {
                _deathLinkService.EnableDeathLink();
                _deathLinkService.OnDeathLinkReceived += ReceiveDeathLink;
            }
            else
            {
                _deathLinkService.DisableDeathLink();
            }
        }

        private void InitializeSlotData(string slotName, Dictionary<string, object> slotDataFields)
        {
            SlotData = new SlotData(slotName, slotDataFields, _console);
        }

        private void InitSession(ArchipelagoConnectionInfo connectionInfo)
        {
            _session = ArchipelagoSessionFactory.CreateSession(connectionInfo.HostUrl,
                connectionInfo.Port);
            _connectionInfo = connectionInfo;
        }

        private void OnMessageReceived(LogMessage message)
        {
            try
            {
                var fullMessage = string.Join(" ", message.Parts.Select(str => str.Text));
                _console.LogInfo(fullMessage);
            }
            catch (Exception ex)
            {
                _console.LogError($"Failed in {nameof(ArchipelagoClient)}.{nameof(OnMessageReceived)}:\n\t{ex}");
                Debugger.Break();
                return; // run original logic
            }
        }

        internal void SetGiftHandler(GiftHandler giftHandler)
        {
            _giftHandler = giftHandler;
        }

        public void SendMessage(string text)
        {
            if (!MakeSureConnected())
            {
                return;
            }
            try
            {

                var packet = new SayPacket()
                {
                    Text = text
                };

                _session.Socket.SendPacket(packet);
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
            }
        }

        private void OnItemReceived(ReceivedItemsHelper receivedItemsHelper)
        {
            try
            {
                if (!MakeSureConnected())
                {
                    return;
                }

                _itemReceivedFunction();
            }
            catch (Exception ex)
            {
                _console.LogError($"Failed in {nameof(ArchipelagoClient)}.{nameof(OnItemReceived)}:\n\t{ex}");
                Debugger.Break();
                return; // run original logic
            }
        }

        public void ReportCheckedLocationsAsync(long[] locationIds)
        {
            var newLocations = locationIds.Except(_session.Locations.AllLocationsChecked).ToArray();
            if (!newLocations.Any())
            {
                return;
            }

            ThreadPool.QueueUserWorkItem((o) => ReportCheckedLocations(newLocations));
            // 
        }

        public void ReportCheckedLocations(long[] locationIds)
        {
            if (!MakeSureConnected())
            {
                return;
            }
            try
            {
                _session.Locations.CompleteLocationChecks(locationIds);
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
            }
        }

        private string GetPlayerName(int playerId)
        {
            return _session.Players.GetPlayerName(playerId) ?? "Archipelago";
        }

        public string GetPlayerAlias(string playerName)
        {
            try
            {
                var player = _session.Players.AllPlayers.FirstOrDefault(x => x.Name == playerName);
                if (player == null || player.Alias == playerName)
                {
                    return null;
                }

                return player.Alias.Substring(0, player.Alias.Length - playerName.Length - 3);
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
                return null;
            }
        }

        public int GetTeam()
        {
            return _session.ConnectionInfo.Team;
        }

        public Dictionary<string, long> GetAllCheckedLocations()
        {
            if (!MakeSureConnected())
            {
                return new Dictionary<string, long>();
            }

            try
            {
                var allLocationsCheckedIds = _session.Locations.AllLocationsChecked;
                var allLocationsChecked = allLocationsCheckedIds.ToDictionary(GetLocationName, x => x);
                return allLocationsChecked;
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
                return new Dictionary<string, long>();
            }
        }

        public List<ReceivedItem> GetAllReceivedItems()
        {
            var allReceivedItems = new List<ReceivedItem>();
            if (!MakeSureConnected())
            {
                return allReceivedItems;
            }

            try
            {
                var apItems = _session.Items.AllItemsReceived.ToArray();
                for (var itemIndex = 0; itemIndex < apItems.Length; itemIndex++)
                {
                    var apItem = apItems[itemIndex];
                    var itemName = GetItemName(apItem.Item);
                    var playerName = GetPlayerName(apItem.Player);
                    var locationName = GetLocationName(apItem.Location) ?? "Thin air";

                    var receivedItem = new ReceivedItem(locationName, itemName, playerName, apItem.Location, apItem.Item,
                        apItem.Player, itemIndex);

                    allReceivedItems.Add(receivedItem);
                }
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
            }

            return allReceivedItems;
        }

        private Dictionary<string, int> GetAllReceivedItemNamesAndCounts()
        {
            if (!MakeSureConnected())
            {
                return new Dictionary<string, int>();
            }

            var receivedItemsGrouped = _session.Items.AllItemsReceived.GroupBy(x => x.Item);
            var receivedItemsWithCount = receivedItemsGrouped.ToDictionary(x => GetItemName(x.First().Item), x => x.Count());
            return receivedItemsWithCount;
        }

        public bool HasReceivedItem(string itemName, out string sendingPlayer)
        {
            sendingPlayer = "";
            if (!MakeSureConnected())
            {
                return false;
            }

            try
            {
                foreach (var receivedItem in _session.Items.AllItemsReceived)
                {
                    if (GetItemName(receivedItem.Item) != itemName)
                    {
                        continue;
                    }

                    sendingPlayer = _session.Players.GetPlayerName(receivedItem.Player);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
            }

            return false;
        }

        public int GetReceivedItemCount(string itemName)
        {
            if (!MakeSureConnected())
            {
                return 0;
            }

            try
            {
                return _session.Items.AllItemsReceived.Count(x => GetItemName(x.Item) == itemName);
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
                return 0;
            }
        }

        private Hint[] GetHints()
        {
            if (!MakeSureConnected())
            {
                return new Hint[0];
            }

            return _session.DataStorage.GetHints();
        }

        public void ReportGoalCompletion()
        {
            if (!MakeSureConnected())
            {
                return;
            }
            try
            {
                _console.LogMessage("Goal Complete!");
                var statusUpdatePacket = new StatusUpdatePacket();
                statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
                _session.Socket.SendPacket(statusUpdatePacket);
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
            }
        }

        private string GetLocationName(long locationId)
        {
            if (!MakeSureConnected())
            {
                return "";
            }

            var locationName = _session.Locations.GetLocationNameFromId(locationId);
            if (string.IsNullOrWhiteSpace(locationName))
            {
                locationName = _localDataPackage.GetLocalLocationName(locationId);
            }

            return locationName;
        }

        public long GetLocationId(string locationName, string gameName = GAME_NAME)
        {
            var locationId = -1L;
            if (MakeSureConnected())
            {
                try
                {
                    locationId = _session.Locations.GetLocationIdFromName(gameName, locationName);
                }
                catch (Exception ex)
                {
                    _console.LogError(ex.Message);
                }
            }
            if (locationId <= 0)
            {
                locationId = _localDataPackage.GetLocalLocationId(locationName);
            }

            return locationId;
        }

        private string GetItemName(long itemId)
        {
            if (!MakeSureConnected())
            {
                return "";
            }

            var itemName = _session.Items.GetItemName(itemId);
            if (string.IsNullOrWhiteSpace(itemName))
            {
                itemName = _localDataPackage.GetLocalItemName(itemId);
            }

            return itemName;
        }

        public void SendDeathLinkAsync(string player, string reason = "Unknown cause")
        {
            Task.Run(() => SendDeathLink(player, reason));
        }

        public void SendDeathLink(string player, string reason = "Unknown cause")
        {
            if (!MakeSureConnected())
            {
                return;
            }

            try
            {
                _deathLinkService.SendDeathLink(new DeathLink(player, reason));
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
            }
        }

        private void ReceiveDeathLink(DeathLink deathlink)
        {
            DiePatch.ReceiveDeathLink();
            var deathLinkMessage = $"You have been killed by {deathlink.Source} ({deathlink.Cause})";
            _console.LogInfo(deathLinkMessage);
        }

        public ScoutedLocation ScoutSingleLocation(string locationName, bool createAsHint = false)
        {
            if (ScoutedLocations.ContainsKey(locationName))
            {
                return ScoutedLocations[locationName];
            }

            if (!MakeSureConnected())
            {
                _console.LogInfo($"Could not find the id for location \"{locationName}\".");
                return null;
            }

            try
            {
                var locationId = GetLocationId(locationName);
                if (locationId == -1)
                {
                    _console.LogInfo($"Could not find the id for location \"{locationName}\".");
                    return null;
                }

                var locationInfo = ScoutLocation(locationId, createAsHint);
                if (locationInfo.Locations.Length < 1)
                {
                    _console.LogInfo($"Could not scout location \"{locationName}\".");
                    return null;
                }

                var firstLocationInfo = locationInfo.Locations[0];
                var itemName = GetItemName(firstLocationInfo.Item);
                var playerSlotName = _session.Players.GetPlayerName(firstLocationInfo.Player);

                var scoutedLocation = new ScoutedLocation(locationName, itemName, playerSlotName, locationId,
                    firstLocationInfo.Item, firstLocationInfo.Player);

                ScoutedLocations.Add(locationName, scoutedLocation);
                return scoutedLocation;
            }
            catch (Exception e)
            {
                _console.LogInfo($"Could not scout location \"{locationName}\". Message: {e.Message}");
                return null;
            }
        }

        private LocationInfoPacket ScoutLocation(long locationId, bool createAsHint)
        {
            var scoutTask = _session.Locations.ScoutLocationsAsync(createAsHint, locationId);
            scoutTask.Wait();
            return scoutTask.Result;
        }

        private void SessionErrorReceived(Exception e, string message)
        {
            _console.LogError($"Connection to Archipelago lost due to receiving a server error. The game will try reconnecting later.\n\tMessage: {message}\n\tException: {e}");
            _lastConnectFailure = DateTime.Now;
            DisconnectAndCleanup();
        }

        private void SessionSocketClosed(string reason)
        {
            _console.LogError($"Connection to Archipelago lost due to the socket closing. The game will try reconnecting later.\n\tReason: {reason}");
            _lastConnectFailure = DateTime.Now;
            DisconnectAndCleanup();
        }

        public void DisconnectAndCleanup()
        {
            if (!IsConnected)
            {
                return;
            }

            if (_session != null)
            {
                _session.Items.ItemReceived -= OnItemReceived;
                _session.MessageLog.OnMessageReceived -= OnMessageReceived;
                _session.Socket.ErrorReceived -= SessionErrorReceived;
                _session.Socket.SocketClosed -= SessionSocketClosed;
                _session.Socket.DisconnectAsync();
            }
            _session = null;
            IsConnected = false;
        }

        public void DisconnectTemporarily()
        {
            DisconnectAndCleanup();
            _allowRetries = false;
        }

        public void ReconnectAfterTemporaryDisconnect()
        {
            _allowRetries = true;
            MakeSureConnected(0);
        }

        public void DisconnectPermanently()
        {
            DisconnectAndCleanup();
            _connectionInfo = null;
        }

        private DateTime _lastConnectFailure;
        private const int THRESHOLD_TO_RETRY_CONNECTION_IN_SECONDS = 15;
        private bool _allowRetries = true;
        public bool MakeSureConnected(int threshold = THRESHOLD_TO_RETRY_CONNECTION_IN_SECONDS)
        {
            if (IsConnected)
            {
                return true;
            }

            if (_connectionInfo == null)
            {
                return false;
            }

            var now = DateTime.Now;
            var timeSinceLastFailure = now - _lastConnectFailure;
            if (timeSinceLastFailure.TotalSeconds < threshold)
            {
                return false;
            }

            if (!_allowRetries)
            {
                _console.LogError("Reconnection attempt failed");
                _lastConnectFailure = DateTime.Now;
                return false;
            }

            TryConnect(_connectionInfo, out _);
            if (!IsConnected)
            {
                _console.LogError("Reconnection attempt failed");
                _lastConnectFailure = DateTime.Now;
                return false;
            }

            _console.LogMessage("Reconnection attempt successful!");
            return IsConnected;
        }

        public void APUpdate()
        {
            if (!MakeSureConnected(60))
            {
                return;
            }

            UpdateReceiveGifts();
        }

        private const int SECONDS_BETWEEN_CHECK_GIFTS = 10;
        private const int FRAMES_BEFORE_CHECK_GIFTS = SECONDS_BETWEEN_CHECK_GIFTS * 60;
        private int _frame = 0;

        private void UpdateReceiveGifts()
        {
            if (_giftHandler == null)
            {
                return;
            }

            _frame = (_frame + 1) % 600;
            if (_frame != 0)
            {
                return;
            }

            _giftHandler.NewGiftNotification();
        }
    }
}
