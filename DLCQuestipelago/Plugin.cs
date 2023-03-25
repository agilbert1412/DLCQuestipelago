using BepInEx;
using BepInEx.NetLauncher.Common;
using DLCLib;
using DLCLib.DLC;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using DLCQuestipelago.Serialization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Notifications;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
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
                    $"Could not connect to archipelago. Please verify the connection file ({Persistency.ConnectionFile}) and that the server is available.";
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
            var jsonString = File.ReadAllText(Persistency.ConnectionFile);
            var connectionInfo = JsonConvert.DeserializeObject<ArchipelagoConnectionInfo>(jsonString);
            if (connectionInfo == null)
            {
                return;
            }

            APConnectionInfo = connectionInfo;
        }

        private void WritePersistentArchipelagoData()
        {
            var jsonObject = JsonConvert.SerializeObject(APConnectionInfo);
            File.WriteAllText(Persistency.ConnectionFile, jsonObject);
        }

        //private TimeSpan _lastTimeSentChecks;
        public void OnUpdateTicked(GameTime gameTime)
        {
            _archipelago.APUpdate();
            // DLCLib.Save.DLCSaveManager.Instance.SaveGameData();
            if (!IsInGame)
            {
                return;
            }
            
            var random = new Random((int)gameTime.TotalGameTime.TotalMilliseconds);
            if (random.NextDouble() < 0.00004)
            {
                var trapIndex = random.Next(0, ItemParser.TrapItems.Length);
                var trap = ItemParser.TrapItems[trapIndex];
                Log.LogInfo($"Creating Trap Item: {trap}");
                _itemManager.ItemParser.ProcessItem(trap);
                SceneManager.Instance.CurrentScene.Update(gameTime);
            }
        }

        public void EnterGame()
        {
            var player = SceneManager.Instance.CurrentScene.Player;
            player.AllowPerformZeldaItem = false;
            _itemManager = new ItemManager(_archipelago, new ReceivedItem[0]);
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
        }

        private void TryShowNotification(string icon)
        {
            NotificationManager.Instance.AddNotification(new Notification()
            {
                Title = "Test Notification",
                Description = $"Icon: {icon}",
                Texture = SceneManager.Instance.CurrentScene.AssetManager.DLCSpriteSheet.Texture,
                SourceRectangle =
                    SceneManager.Instance.CurrentScene.AssetManager.DLCSpriteSheet.SourceRectangle(icon),
                Tint = Color.White,
                CueName = "toast_up"
            });
        }
    }
}
