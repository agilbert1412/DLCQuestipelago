using DLCQuestipelago.Archipelago;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using DLCLib;
using DLCLib.Campaigns;
using DLCLib.Save;
using EasyStorage;
using Newtonsoft.Json;

namespace DLCQuestipelago.Items
{
    public class ItemManager
    {
        private const string ITEM_PERSISTENCY_FILENAME = "ArchipelagoPersistency.json";

        private ArchipelagoClient _archipelago;
        public ItemParser ItemParser { get; }
        private HashSet<ReceivedItem> _itemsAlreadyProcessed;
        private HashSet<ReceivedItem> _itemsAlreadyProcessedThisRun;

        public ItemManager(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
            ItemParser = new ItemParser(archipelago);
            _itemsAlreadyProcessed = LoadItemsAlreadyProcessedFromCampaign();
            _itemsAlreadyProcessedThisRun = new HashSet<ReceivedItem>();
        }

        public List<ReceivedItem> GetAllItemsAlreadyProcessed()
        {
            return _itemsAlreadyProcessed.ToList();
        }

        public void ReceiveAllNewItems()
        {
            var allReceivedItems = _archipelago.GetAllReceivedItems();

            foreach (var receivedItem in allReceivedItems)
            {
                ReceiveNewItem(receivedItem);
            }
        }

        private void ReceiveNewItem(ReceivedItem receivedItem)
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
                _itemsAlreadyProcessed.Add(receivedItem);
            }
        }


        private static MethodInfo GetSaveFilenameMethod = typeof(DLCSaveManager).GetMethod("GetSaveFilename", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo SaveDeviceField = typeof(DLCSaveManager).GetField("saveDevice", BindingFlags.NonPublic | BindingFlags.Instance);

        private string GetPersistencyFileName()
        {
            var saveFileName = (string)GetSaveFilenameMethod.Invoke(DLCSaveManager.Instance, new object[0]);
            var itemPersistencyFile = saveFileName + "_" + ITEM_PERSISTENCY_FILENAME;
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
