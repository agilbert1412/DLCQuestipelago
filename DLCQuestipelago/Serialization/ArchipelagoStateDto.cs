using System.Collections.Generic;
using DLCQuestipelago.Archipelago;

namespace DLCQuestipelago.Serialization
{
    public class ArchipelagoStateDto
    {
        public List<ReceivedItem> ItemsReceived { get; set; }
        public List<string> LocationsChecked { get; set; }
        public Dictionary<string, ScoutedLocation> LocationsScouted { get; set; }

        public ArchipelagoStateDto()
        {
            ItemsReceived = new List<ReceivedItem>();
            LocationsChecked = new List<string>();
            LocationsScouted = new Dictionary<string, ScoutedLocation>();
        }
    }
}
