using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx.Logging;
using DLCLib;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.Archipelago
{
    public class ArchipelagoClient
    {
        private const string GAME_NAME = "DLCquest";
        private ManualLogSource _console;
        private ArchipelagoSession _session;
        private DeathLinkService _deathLinkService;
        private Harmony _harmony;
        private DeathManager _deathManager;
        private ArchipelagoConnectionInfo _connectionInfo;

        private Action _itemReceivedFunction;

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
                var minimumVersion = new Version(0, 3, 9);
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
                return;
            }
            _session.Items.ItemReceived += OnItemReceived;
            _session.MessageLog.OnMessageReceived += OnMessageReceived;
            _session.Socket.ErrorReceived += SessionErrorReceived;
            _session.Socket.SocketClosed += SessionSocketClosed;

            InitializeDeathLink();

            IsConnected = true;
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
            if (_deathManager == null)
            {
                _deathManager = new DeathManager(_console, _harmony, this);
                _deathManager.HookIntoDeathlinkEvents();
            }

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
            var fullMessage = string.Join(" ", message.Parts.Select(str => str.Text));
            _console.LogInfo(fullMessage);
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
            if (!MakeSureConnected())
            {
                return;
            }
            
            _itemReceivedFunction();
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

        private string GetPlayerAlias(string playerName)
        {
            var player = _session.Players.AllPlayers.FirstOrDefault(x => x.Name == playerName);
            if (player == null)
            {
                return null;
            }

            return player.Alias;
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
            DeathManager.ReceiveDeathLink();
            var deathLinkMessage = $"You have been killed by {deathlink.Source} ({deathlink.Cause})";
            _console.LogInfo(deathLinkMessage);
        }

        private ScoutedLocation ScoutSingleLocation(string locationName, bool createAsHint = false)
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
            _console.LogError($"Connection to Archipelago lost. The game will try reconnecting later. Message: {message}");
            _lastConnectFailure = DateTime.Now;
            DisconnectAndCleanup();
        }

        private void SessionSocketClosed(string reason)
        {
            _console.LogError($"Connection to Archipelago lost. The game will try reconnecting later. Message: {reason}");
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

        public void DisconnectPermanently()
        {
            DisconnectAndCleanup();
            _connectionInfo = null;
        }

        private DateTime _lastConnectFailure;
        private const int THRESHOLD_TO_RETRY_CONNECTION_IN_SECONDS = 15;
        public bool MakeSureConnected(int threshold = THRESHOLD_TO_RETRY_CONNECTION_IN_SECONDS)
        {
            if (IsConnected && _session != null && _session.Socket != null && _session.Socket.Connected)
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
            MakeSureConnected(60);
        }

        public const string STRING_DATA_STORAGE_DELIMITER = "|||";
        public void AddToStringDataStorage(string key, string value)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            var existingValue = ReadStringFromDataStorage(key);
            if (string.IsNullOrWhiteSpace(existingValue))
            {
                _session.DataStorage[Scope.Game, key] = value;
            }
            else
            {
                _session.DataStorage[Scope.Game, key] = existingValue + STRING_DATA_STORAGE_DELIMITER + value;
            }
        }

        public void SetStringDataStorage(string key, string value)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            try
            {
                _session.DataStorage[Scope.Game, key] = value;
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
            }
        }

        public bool StringExistsInDataStorage(string key)
        {
            if (!MakeSureConnected())
            {
                return false;
            }

            var value = _session.DataStorage[Scope.Game, key];
            return !string.IsNullOrWhiteSpace(value.To<string>());
        }

        public string ReadStringFromDataStorage(string key)
        {
            if (!MakeSureConnected())
            {
                return null;
            }

            try
            {
                var value = _session.DataStorage[Scope.Game, key];
                var stringValue = value.To<string>();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }

                return stringValue;
            }
            catch (Exception ex)
            {
                _console.LogError(ex.Message);
                return null;
            }
        }

        public void RemoveStringFromDataStorage(string key)
        {
            if (!MakeSureConnected())
            {
                return;
            }

            _session.DataStorage[Scope.Game, key] = "";
        }
    }
}
