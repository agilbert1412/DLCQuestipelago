﻿using BepInEx;
using DLCLib;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
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

namespace DLCQuestipelago
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        private const string CONNECT_SYNTAX = "Syntax: connect ip:port slot password";
        public static Plugin Instance;
        public static DLCDualContentManager DualContentManager;
        public static DLCDualAssetManager DualAssetManager;

        private Harmony _harmony;
        private ArchipelagoClient _archipelago;
        public ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        private LocationChecker _locationChecker;
        private ItemManager _itemManager;
        private ObjectivePersistence _objectivePersistence;
        private ArchipelagoNotificationsHandler _notificationHandler;
        private TrapManager _trapManager;
        private SpeedChanger _speedChanger;

        private GiftHandler _giftHandler;

        public bool IsInGame { get; private set; }

        public override void Load()
        {
            Log.LogInfo($"Loading {PluginInfo.PLUGIN_NAME}...");
            Instance = this;

            try
            {
                _harmony = new Harmony(PluginInfo.PLUGIN_NAME);
                _harmony.PatchAll();
            }
            catch (FileNotFoundException fnfe)
            {
                if (fnfe.FileName.Contains("Microsoft.Xna.Framework"))
                    Log.LogError($"Cannot load {PluginInfo.PLUGIN_NAME}: Microsoft XNA Framework 4.0 is not installed. Please run DLC Quest from Steam, then try again.");
                throw;
            }

            TaskExtensions.Initialize(Log);
            _archipelago = new ArchipelagoClient(Log, _harmony, OnItemReceived);
            _notificationHandler = new ArchipelagoNotificationsHandler(Log, _archipelago);
            _speedChanger = new SpeedChanger(Log);
            _trapManager = new TrapManager(_archipelago);
            DLCContentManagerInitializePatch.Initialize(Log, _notificationHandler);
            ConnectToArchipelago();
            IsInGame = false;

            CampaignSelectPatch.Initialize(Log, _archipelago);
            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} is loaded!");
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
            // _chatForwarder = new ChatForwarder(Monitor, _harmony, _giftHandler, _appearanceRandomizer);

            if (APConnectionInfo != null && !_archipelago.IsConnected)
            {
                _archipelago.Connect(APConnectionInfo, out var errorMessage);
            }

            if (!_archipelago.IsConnected)
            {
                APConnectionInfo = null;
                var userMessage =
                    $"Could not connect to archipelago. Please verify the connection file ({Persistency.CONNECTION_FILE}) and that the server is available.";
                Log.LogError(userMessage);
                Console.ReadKey();
                Environment.Exit(0);
                return;
            }

            // _chatForwarder.ListenToChatMessages(_archipelago);
            Log.LogMessage($"Connected to Archipelago as {_archipelago.SlotData.SlotName}.");// Type !!help for client commands");
            WritePersistentArchipelagoData();
            _giftHandler = new GiftHandler(Log, _archipelago, _notificationHandler, _trapManager, _speedChanger);
        }

        private void ReadPersistentArchipelagoData()
        {
            if (!File.Exists(Persistency.CONNECTION_FILE))
            {
                var defaultConnectionInfo = new ArchipelagoConnectionInfo("archipelago.gg", 38281, "Name", false);
                WritePersistentData(defaultConnectionInfo, Persistency.CONNECTION_FILE);
            }

            var jsonString = File.ReadAllText(Persistency.CONNECTION_FILE);
            var connectionInfo = JsonConvert.DeserializeObject<ArchipelagoConnectionInfo>(jsonString);
            if (connectionInfo == null)
            {
                return;
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
        }

        public void EnterGame()
        {
            var player = SceneManager.Instance.CurrentScene.Player;
            player.AllowPerformZeldaItem = false;
            _itemManager = new ItemManager(Log, _archipelago, _notificationHandler, _trapManager);
            _locationChecker = new LocationChecker(Log, _archipelago, new List<string>());
            _objectivePersistence = new ObjectivePersistence(Log, _archipelago);
            _notificationHandler.InitializeTextures();

            _locationChecker.VerifyNewLocationChecksWithArchipelago();
            _locationChecker.SendAllLocationChecks();
            _itemManager.ReceiveAllNewItems();

            PatcherInitializer.Initialize(Log, _archipelago, _locationChecker, _itemManager, _objectivePersistence, _giftHandler.Sender);

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
                Log.LogError($"Failed at properly syncing with archipelago when exiting game. Message: {ex.Message}");
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
