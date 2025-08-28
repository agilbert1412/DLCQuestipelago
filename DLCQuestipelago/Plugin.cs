using BepInEx;
using DLCLib;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Items;
using KaitoKid.ArchipelagoUtilities.Net;
using DLCQuestipelago.Serialization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.NET.Common;
using DLCQuestipelago.DualContentManager;
using DLCQuestipelago.Extensions;
using DLCQuestipelago.Gifting;
using DLCQuestipelago.MoveLink;
using DLCQuestipelago.Utilities;
using KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static Plugin Instance;
        public static DLCDualContentManager DualContentManager;
        public static DLCDualAssetManager DualAssetManager;

        private ILogger _logger;
        private Harmony _harmony;
        private DLCQArchipelagoClient _archipelago;
        public DLCQuestConnectionInfo APConnectionInfo { get; set; }
        private LocationChecker _locationChecker;
        private DLCQItemManager _itemManager;
        private ObjectivePersistence _objectivePersistence;
        private ArchipelagoNotificationsHandler _notificationHandler;
        private TrapManager _trapManager;
        private SpeedChanger _speedChanger;

        private GiftHandler _giftHandler;

        public bool IsInGame { get; private set; }
        public string UniqueIdentifier = Guid.NewGuid().ToString();

        public override void Load()
        {
            Log.LogInfo($"Loading {PluginInfo.PLUGIN_NAME}...");
            Instance = this;

            _logger = new LogHandler(Log);

            try
            {
                _harmony = new Harmony(PluginInfo.PLUGIN_NAME);
                _harmony.PatchAll();
            }
            catch (FileNotFoundException fnfe)
            {
                if (fnfe.FileName.Contains("Microsoft.Xna.Framework"))
                    _logger.LogError($"Cannot load {PluginInfo.PLUGIN_NAME}: Microsoft XNA Framework 4.0 is not installed. Please run DLC Quest from Steam, then try again.");
                throw;
            }

            TaskExtensions.Initialize(_logger);
            _archipelago = new DLCQArchipelagoClient(_logger, OnItemReceived);
            _notificationHandler = new ArchipelagoNotificationsHandler(_logger, _archipelago);
            _speedChanger = new SpeedChanger(_logger);
            _trapManager = new TrapManager(_archipelago);
            DLCContentManagerInitializePatch.Initialize(_logger, _notificationHandler);
            ConnectToArchipelago();
            IsInGame = false;

            CampaignSelectPatch.Initialize(_logger, _archipelago.SlotData);
            _logger.LogInfo($"{PluginInfo.PLUGIN_NAME} is loaded!");
        }

        public override bool Unload()
        {
            _giftHandler?.CloseGiftBox();
            _giftHandler = null;
            return base.Unload();
        }

        public void SaveAndQuit()
        {
            SaveGame();
            ExitGame();
        }

        private void ConnectToArchipelago()
        {
            ReadPersistentArchipelagoData();

            var errorMessage = "";
            if (APConnectionInfo != null && !_archipelago.IsConnected)
            {
                var connectionResult = _archipelago.ConnectToMultiworld(APConnectionInfo);
                errorMessage = connectionResult.Message;
            }

            if (!_archipelago.IsConnected)
            {
                APConnectionInfo = null;
                var userMessage = $"Could not connect to archipelago.{Environment.NewLine}Message: {errorMessage}{Environment.NewLine}Please verify the connection file ({Persistency.CONNECTION_FILE}) and that the server is available.{Environment.NewLine}";
                _logger.LogError(userMessage);
                Console.ReadKey();
                Environment.Exit(0);
                return;
            }

            // _chatForwarder.ListenToChatMessages(_archipelago);
            _logger.LogMessage($"Connected to Archipelago as {_archipelago.SlotData.SlotName}.");// Type !!help for client commands");
            WritePersistentArchipelagoData();
            _giftHandler = new GiftHandler(_logger, _archipelago, _notificationHandler, _trapManager, _speedChanger);
            PatcherInitializer.InitializeEarly(_logger, _archipelago);
        }

        private void ReadPersistentArchipelagoData()
        {
            if (!File.Exists(Persistency.CONNECTION_FILE))
            {
                var defaultConnectionInfo = new DLCQuestConnectionInfo("archipelago.gg", 38281, "Name", false);
                WritePersistentData(defaultConnectionInfo, Persistency.CONNECTION_FILE);
            }

            var jsonString = File.ReadAllText(Persistency.CONNECTION_FILE);
            var connectionInfo = JsonConvert.DeserializeObject<DLCQuestConnectionInfo>(jsonString);
            if (connectionInfo == null)
            {
                return;
            }

            // Options added in 3.4.0
            if (string.IsNullOrWhiteSpace(connectionInfo.TeleportToSpawnKey))
            {
                connectionInfo.TeleportToSpawnKey = "S";
                connectionInfo.EnableEnergyLink = true;
            }

            APConnectionInfo = connectionInfo;
        }

        private void WritePersistentArchipelagoData()
        {
            WritePersistentData(APConnectionInfo, Persistency.CONNECTION_FILE);
        }

        private void WritePersistentData(object data, string path)
        {
            var jsonObject = JsonConvert.SerializeObject(data, Formatting.Indented, new GiftingModJsonConverter());
            File.WriteAllText(path, jsonObject);
        }

        //private TimeSpan _lastTimeSentChecks;
        public void OnUpdateTicked(GameTime gameTime)
        {
            if (!IsInGame || _archipelago == null)
            {
                return;
            }
            
            _archipelago.APUpdate();
            // DLCLib.Save.DLCSaveManager.Instance.SaveGameData();

            MoveLinkManager.UpdateMove(gameTime);
        }

        public void EnterGame()
        {
            var player = SceneManager.Instance.CurrentScene.Player;
            player.AllowPerformZeldaItem = false;
            _itemManager = new DLCQItemManager(_logger, _archipelago, _notificationHandler, _trapManager);
            _locationChecker = new LocationChecker(_logger, _archipelago, new List<string>());
            _objectivePersistence = new ObjectivePersistence(_logger, _archipelago);
            _notificationHandler.InitializeTextures();

            _locationChecker.VerifyNewLocationChecksWithArchipelago();
            _locationChecker.SendAllLocationChecks();
            _itemManager.ReceiveAllNewItems();

            PatcherInitializer.Initialize(_logger, _archipelago, _locationChecker, _itemManager, _objectivePersistence, _giftHandler.Sender);

            IsInGame = true;
            player.AllowPerformZeldaItem = true;
            CoinsanityUtils.UpdateCoinsUI();
            player.RefreshAnimations();

            CoinPickupPatch.CheckAllCoinsanityLocations(player.Inventory);

            _speedChanger.ResetPlayerSpeedToDefault();
#if DEBUG
            _speedChanger.AddMultiplierToPlayerSpeed(1.5f);
#endif

            _giftHandler.OpenGiftBox();
    }

        public void SaveGame()
        {
            try
            {
                if (_locationChecker == null)
                {
                    return;
                }

                _locationChecker.VerifyNewLocationChecksWithArchipelago();
                _locationChecker.SendAllLocationChecks();
                _itemManager.SaveItemsAlreadyProcessedToCampaign();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed at properly syncing with archipelago when exiting game. Message: {ex.Message}");
                return;
            }
        }

        public void ExitGame()
        {
            _giftHandler.CloseGiftBox();
            _itemManager = null;
            _locationChecker = null;
            _objectivePersistence = null;

            IsInGame = false;
        }

        private void OnItemReceived()
        {
            if (!IsInGame || _archipelago == null)
            {
                return;
            }

            _itemManager.ReceiveAllNewItems();

            CoinsanityUtils.UpdateCoinsUI();
            SceneManager.Instance.CurrentScene.Player.RefreshAnimations();
        }
    }
}
