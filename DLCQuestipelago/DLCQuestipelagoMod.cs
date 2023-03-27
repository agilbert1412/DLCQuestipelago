using DLCLib;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using DLCQuestipelago.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DLCQuestipelago.DualContentManager;
using MelonLoader;

namespace DLCQuestipelago
{
    public class DLCQuestipelagoMod : MelonMod
    {
        private const string CONNECT_SYNTAX = "Syntax: connect ip:port slot password";
        public static DLCQuestipelagoMod Instance;
        public static DLCDualContentManager DualContentManager;
        public static DLCDualAssetManager DualAssetManager;
        
        private ArchipelagoClient _archipelago;
        private Logger _logger;
        public ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        private LocationChecker _locationChecker;
        private ItemManager _itemManager;
        private ObjectivePersistence _objectivePersistence;
        private ArchipelagoNotificationsHandler _notificationHandler;

        public bool IsInGame { get; private set; }

        public override void OnInitializeMelon()
        {
            _logger = new Logger(LoggerInstance);
            _logger.LogInfo($"Loading {Info.Name}...");
            Instance = this;
            
            HarmonyInstance.PatchAll();

            _archipelago = new ArchipelagoClient(_logger, HarmonyInstance, OnItemReceived);
            _notificationHandler = new ArchipelagoNotificationsHandler(_logger, _archipelago);
            DLCContentManagerInitializePatch.Initialize(_logger, _notificationHandler);
            ConnectToArchipelago();
            IsInGame = false;

            CampaignSelectPatch.Initialize(_logger, _archipelago);
            _logger.LogInfo($"{Info.Name} is loaded!");
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
                _logger.LogError(userMessage);
                Console.ReadKey();
                Environment.Exit(0);
                return;
            }

            // _chatForwarder.ListenToChatMessages(_archipelago);
            _logger.LogInfo($"Connected to Archipelago as {_archipelago.SlotData.SlotName}.");// Type !!help for client commands");
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
            
            /*var random = new Random((int)gameTime.TotalGameTime.TotalMilliseconds);
            if (random.NextDouble() < 0.00004)
            {
                var trapIndex = random.Next(0, ItemParser.TrapItems.Length);
                var trap = ItemParser.TrapItems[trapIndex];
                Log.LogInfo($"Creating Trap Item: {trap}");
                _itemManager.ItemParser.ProcessItem(trap);
                SceneManager.Instance.CurrentScene.Update(gameTime);
            }*/
        }

        public void EnterGame()
        {
            var player = SceneManager.Instance.CurrentScene.Player;
            player.AllowPerformZeldaItem = false;
            _itemManager = new ItemManager(_archipelago);
            _locationChecker = new LocationChecker(_logger, _archipelago, new List<string>());
            _objectivePersistence = new ObjectivePersistence(_archipelago);

            _locationChecker.VerifyNewLocationChecksWithArchipelago();
            _locationChecker.SendAllLocationChecks();
            _itemManager.ReceiveAllNewItems();

            PatcherInitializer.Initialize(_logger, _archipelago, _locationChecker, _itemManager, _objectivePersistence);

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
            _logger.LogInfo($"Received Item: {lastReceivedItem}");
            _itemManager.ReceiveAllNewItems();

            _notificationHandler.AddNotification(lastReceivedItem);

            InventoryCoinsGetPatch.UpdateCoinsUI();
            SceneManager.Instance.CurrentScene.Player.RefreshAnimations();
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
