using DLCQuestipelago.Gifting;
using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace DLCQuestipelago.Archipelago
{
    public class DLCQuestConnectionInfo : ArchipelagoConnectionInfo
    {
        public GiftingMode GiftingPreference { get; set; } = GiftingMode.Strategic;
        public bool EnableEnergyLink { get; set; } = true;
        public string TeleportToSpawnKey { get; set; } = "A";

        public DLCQuestConnectionInfo(string hostUrl, int port, string slotName, bool? deathLink, string password = null, GiftingMode giftingPreference = GiftingMode.Strategic, bool enableEnergyLink = true, string teleportToSpawnKey = "A") : base(hostUrl, port, slotName, deathLink, password)
        {
            GiftingPreference = giftingPreference;
            EnableEnergyLink = enableEnergyLink;
            TeleportToSpawnKey = teleportToSpawnKey;
        }
    }
}
