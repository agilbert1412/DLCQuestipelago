using BepInEx;
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
using System.Linq;
using System.Reflection;
using BepInEx.NET.Common;
using DLCQuestipelago.DualContentManager;

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

        public bool IsInGame { get; private set; }

        public override void Load()
        {
            Log.LogInfo($"Loading {PluginInfo.PLUGIN_NAME}...");
            Instance = this;

            _harmony = new Harmony(PluginInfo.PLUGIN_NAME);
            _harmony.PatchAll();

            _archipelago = new ArchipelagoClient(Log, _harmony, OnItemReceived);
            _notificationHandler = new ArchipelagoNotificationsHandler(Log, _archipelago);
            DLCContentManagerInitializePatch.Initialize(Log, _notificationHandler);
            ConnectToArchipelago();
            IsInGame = false;

            CampaignSelectPatch.Initialize(Log, _archipelago);
            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} is loaded!");
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
            var jsonObject = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, jsonObject);
        }

        //private TimeSpan _lastTimeSentChecks;
        public void OnUpdateTicked(GameTime gameTime)
        {
            _archipelago.APUpdate();
            if (!IsInGame)
            {
                return;
            }
            // DLCLib.Save.DLCSaveManager.Instance.SaveGameData();
        }

        public void EnterGame()
        {
            var player = SceneManager.Instance.CurrentScene.Player;
            player.AllowPerformZeldaItem = false;
            _itemManager = new ItemManager(_archipelago);
            _locationChecker = new LocationChecker(Log, _archipelago, new List<string>());
            _objectivePersistence = new ObjectivePersistence(_archipelago);

            _locationChecker.VerifyNewLocationChecksWithArchipelago();
            _locationChecker.SendAllLocationChecks();
            _itemManager.ReceiveAllNewItems();

            PatcherInitializer.Initialize(Log, _archipelago, _locationChecker, _itemManager, _objectivePersistence);

            IsInGame = true;
            player.AllowPerformZeldaItem = true;
            InventoryCoinsGetPatch.UpdateCoinsUI();
            player.RefreshAnimations();

            CoinPickupPatch.CheckAllCoinsanityLocations(player.Inventory);
#if DEBUG
            var inputGroundField = typeof(Player).GetField("PLAYER_INPUT_SCALE_GROUND", BindingFlags.NonPublic | BindingFlags.Static);
            inputGroundField.SetValue(null, 45f * 1.5f);
            var inputAirField = typeof(Player).GetField("PLAYER_INPUT_SCALE_AIR", BindingFlags.NonPublic | BindingFlags.Static);
            inputAirField.SetValue(null, 30f * 1.5f);
            // private static float PLAYER_INPUT_SCALE_GROUND = 45f;
            // private static float PLAYER_INPUT_SCALE_AIR = 30f;
#endif
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
            _itemManager = null;
            _locationChecker = null;
            _objectivePersistence = null;

            IsInGame = false;
        }

        private void OnItemReceived()
        {
            if (!IsInGame)
            {
                return;
            }

            var lastReceivedItem = _archipelago.GetAllReceivedItems().Last().ItemName;
            Log.LogMessage($"Received Item: {lastReceivedItem}");
            _itemManager.ReceiveAllNewItems();

            _notificationHandler.AddNotification(lastReceivedItem);

            InventoryCoinsGetPatch.UpdateCoinsUI();
            SceneManager.Instance.CurrentScene.Player.RefreshAnimations();
        }
    }
}
