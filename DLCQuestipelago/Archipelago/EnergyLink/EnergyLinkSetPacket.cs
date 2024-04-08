using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;

namespace DLCQuestipelago.Archipelago.EnergyLink;

class EnergyLinkSetPacket : SetPacket
{
    [JsonProperty("slot")]
    public int Slot { get; set; }
}