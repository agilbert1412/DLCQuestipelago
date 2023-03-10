using System.IO;

namespace DLCQuestipelago.Serialization
{
    public static class Persistency
    {
        public static readonly string ConnectionFile = Path.Combine("BepInEx", "plugins", "DLCQuestipelago", "ArchipelagoConnectionInfo.json");
    }
}
