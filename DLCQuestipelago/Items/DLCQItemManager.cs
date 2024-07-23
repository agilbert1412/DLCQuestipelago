using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DLCLib.Save;
using EasyStorage;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;

namespace DLCQuestipelago.Items
{
    public class DLCQItemManager : ItemManager
    {
        private const string ITEM_PERSISTENCY_FILENAME = "ArchipelagoPersistency.json";

        private ILogger _logger;
        private ArchipelagoNotificationsHandler _notificationHandler;
        public ItemParser ItemParser { get; }
        private HashSet<ReceivedItem> _itemsAlreadyProcessedThisRun;

        public DLCQItemManager(ILogger logger, ArchipelagoClient archipelago, ArchipelagoNotificationsHandler notificationHandler, TrapManager trapManager) : base(archipelago, new ReceivedItem[0])
        {
            _logger = logger;
            _notificationHandler = notificationHandler;

            ItemParser = new ItemParser(archipelago, trapManager);
            _itemsAlreadyProcessed = LoadItemsAlreadyProcessedFromCampaign();
            _itemsAlreadyProcessedThisRun = new HashSet<ReceivedItem>();
        }

        protected override void ProcessItem(ReceivedItem receivedItem, bool immediatelyIfPossible)
        {
            throw new System.NotImplementedException();
        }

        protected override void ReceiveNewItem(ReceivedItem receivedItem, bool immediatelyIfPossible)
        {
            if (_itemsAlreadyProcessedThisRun.Contains(receivedItem))
            {
                return;
            }

            var isNew = !_itemsAlreadyProcessed.Contains(receivedItem);
            ItemParser.ProcessItem(receivedItem, isNew);
            _itemsAlreadyProcessedThisRun.Add(receivedItem);

            if (isNew)
            {
                _logger.LogMessage($"Item received: {receivedItem.ItemName}");
                _notificationHandler.AddItemNotification(receivedItem.ItemName);
                _itemsAlreadyProcessed.Add(receivedItem);
            }
        }

        private static MethodInfo GetSaveFilenameMethod = typeof(DLCSaveManager).GetMethod("GetSaveFilename", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo SaveDeviceField = typeof(DLCSaveManager).GetField("saveDevice", BindingFlags.NonPublic | BindingFlags.Instance);

        private string GetPersistencyFileName()
        {
            var saveFileName = (string)GetSaveFilenameMethod.Invoke(DLCSaveManager.Instance, new object[0]);
            var itemPersistencyFile = saveFileName.Replace(".xml", "") + "_" + ITEM_PERSISTENCY_FILENAME;
            return itemPersistencyFile; 
        }

        internal void SaveItemsAlreadyProcessedToCampaign()
        {
            var allItemsAlreadyProcessed = GetAllItemsAlreadyProcessed();
            var itemsAsJson = JsonConvert.SerializeObject(allItemsAlreadyProcessed);
            var saveDevice = (SaveDevice)SaveDeviceField.GetValue(DLCSaveManager.Instance);
            saveDevice.Save(DLCSaveManager.Instance.DataSaveDirectory, GetPersistencyFileName(), (stream) =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(itemsAsJson);
                    writer.Flush();
                }
            });
        }

        private HashSet<ReceivedItem> LoadItemsAlreadyProcessedFromCampaign()
        {
            var saveDevice = (SaveDevice)SaveDeviceField.GetValue(DLCSaveManager.Instance);
            var saveDirectory = DLCSaveManager.Instance.DataSaveDirectory;
            var persistencyFileName = GetPersistencyFileName();
            var fileExists = saveDevice.FileExists(saveDirectory, persistencyFileName);
            if (!fileExists)
            {
                return new HashSet<ReceivedItem>();
            }

            string itemsAsJson = null;
            saveDevice.Load(saveDirectory, persistencyFileName, (stream) =>
            {
                using (var reader = new StreamReader(stream))
                {
                    itemsAsJson = reader.ReadToEnd();
                }
            });

            var itemsAlreadyProcessed = JsonConvert.DeserializeObject<List<ReceivedItem>>(itemsAsJson);
            return new HashSet<ReceivedItem>(itemsAlreadyProcessed);
        }
    }
}
