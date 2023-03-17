using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace DLCQuestipelago.Archipelago
{
    public class ArchipelagoLocation
    {
        public string Name { get; set; }
        public long Id { get; set; }

        public ArchipelagoLocation(string name, long id)
        {
            Name = name;
            Id = id;
        }

        public static IEnumerable<ArchipelagoLocation> LoadLocations()
        {
            var pathToLocationTable = Path.Combine("BepInEx", "plugins", "DLCQuestipelago", "IdTables", "dlc_quest_location_table.json");
            var jsonContent = File.ReadAllText(pathToLocationTable);
            var locationsTable = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(jsonContent);
            var locations = locationsTable["locations"];
            foreach (var locationJson in locations)
            {
                yield return LoadLocation(locationJson.Key, locationJson.Value);
            }
        }

        private static ArchipelagoLocation LoadLocation(string locationName, JToken locationJson)
        {
            var id = locationJson.Value<long>();
            var location = new ArchipelagoLocation(locationName, id);
            return location;
        }
    }
}
