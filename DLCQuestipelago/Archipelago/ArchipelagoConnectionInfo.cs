using DLCQuestipelago.Gifting;

namespace DLCQuestipelago.Archipelago
{
    public class ArchipelagoConnectionInfo
    {
        public string HostUrl { get; private set; }
        public int Port { get; private set; }
        public string SlotName { get; private set; }
        public bool DeathLink { get; set; }
        public string Password { get; private set; }
        public GiftingMode GiftingPreference { get; private set; } = GiftingMode.Strategic;

        public ArchipelagoConnectionInfo(string hostUrl, int port, string slotName, bool deathLink, string password = null, GiftingMode giftingPreference = GiftingMode.Strategic)
        {
            HostUrl = hostUrl;
            Port = port;
            SlotName = slotName;
            DeathLink = deathLink;
            Password = password;
            GiftingPreference = giftingPreference;
        }
    }
}