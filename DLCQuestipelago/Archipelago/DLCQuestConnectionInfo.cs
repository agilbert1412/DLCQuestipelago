using DLCQuestipelago.Gifting;
using DLCQuestipelago.QualityOfLife;
using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace DLCQuestipelago.Archipelago
{
    public class DLCQuestConnectionInfo : ArchipelagoConnectionInfo
    {
        public GiftingMode GiftingPreference { get; set; } = GiftingMode.Strategic;
        public bool EnableEnergyLink { get; set; } = true;
        public string TeleportToSpawnKey { get; set; } = InputPatch.DEFAULT_TELEPORT_SPAWN_KEY;
        public string CutsceneSkipKey { get; set; } = CutsceneSkipperPatch.DEFAULT_CUTSCENE_SKIP_KEY;

        public DLCQuestConnectionInfo(string hostUrl, int port, string slotName, bool? deathLink, string password = null, GiftingMode giftingPreference = GiftingMode.Strategic, bool enableEnergyLink = true, string teleportToSpawnKey = InputPatch.DEFAULT_TELEPORT_SPAWN_KEY, string cutsceneSkipKey = CutsceneSkipperPatch.DEFAULT_CUTSCENE_SKIP_KEY) : base(hostUrl, port, slotName, deathLink, password)
        {
            GiftingPreference = giftingPreference;
            EnableEnergyLink = enableEnergyLink;
            TeleportToSpawnKey = teleportToSpawnKey;
            CutsceneSkipKey = cutsceneSkipKey;
        }
    }
}
