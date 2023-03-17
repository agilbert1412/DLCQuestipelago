using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace DLCQuestipelago.Archipelago
{
    public class ArchipelagoItem
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public ItemClassification Classification { get; set; }

        public ArchipelagoItem(string name, long id, ItemClassification classification)
        {
            Name = name;
            Id = id;
            Classification = classification;
        }

        public static IEnumerable<ArchipelagoItem> LoadItems()
        {
            var pathToItemTable = Path.Combine("BepInEx", "plugins", "DLCQuestipelago", "IdTables", "dlc_quest_item_table.json");
            var jsonContent = File.ReadAllText(pathToItemTable);
            var itemsTable = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(jsonContent);
            var items = itemsTable["items"];
            foreach (var itemJson in items)
            {
                yield return LoadItem(itemJson.Key, itemJson.Value);
            }
        }

        private static ArchipelagoItem LoadItem(string itemName, JToken itemJson)
        {
            var id = itemJson["code"].Value<long>();
            var classification = (ItemClassification)Enum.Parse(typeof(ItemClassification), itemJson["classification"].Value<string>(), true);
            var item = new ArchipelagoItem(itemName, id, classification);
            return item;
        }
    }

    public enum ItemClassification
    {
        Progression,
        Useful,
        Filler,
        Trap,
    }
}
