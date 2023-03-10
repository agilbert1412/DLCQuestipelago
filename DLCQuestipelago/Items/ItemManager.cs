using System.Collections.Generic;
using System.Linq;
using DLCQuestipelago.Archipelago;

namespace DLCQuestipelago.Items
{
    public class ItemManager
    {
        private ArchipelagoClient _archipelago;
        public ItemParser ItemParser { get; }
        private HashSet<ReceivedItem> _itemsAlreadyProcessed;

        public ItemManager(ArchipelagoClient archipelago, IEnumerable<ReceivedItem> itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            ItemParser = new ItemParser();
            _itemsAlreadyProcessed = new HashSet<ReceivedItem>(itemsAlreadyProcessed);
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
            if (_itemsAlreadyProcessed.Contains(receivedItem))
            {
                return;
            }

            ItemParser.ProcessItem(receivedItem);
            _itemsAlreadyProcessed.Add(receivedItem);
        }
    }
}
