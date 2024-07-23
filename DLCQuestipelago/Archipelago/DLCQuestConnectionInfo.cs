using DLCQuestipelago.Gifting;
using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace DLCQuestipelago.Archipelago
{
    public class DLCQuestConnectionInfo : ArchipelagoConnectionInfo
    {
        public GiftingMode GiftingPreference { get; private set; } = GiftingMode.Strategic;

        public DLCQuestConnectionInfo(string hostUrl, int port, string slotName, bool? deathLink, string password = null, GiftingMode giftingPreference = GiftingMode.Strategic) : base(hostUrl, port, slotName, deathLink, password)
        {
            GiftingPreference = giftingPreference;
        }
    }
}
