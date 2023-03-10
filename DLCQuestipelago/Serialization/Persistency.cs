using System.IO;

namespace DLCQuestipelago.Serialization
{
    public static class Persistency
    {
        public static readonly string ConnectionFile = Path.Combine("BepInEx", "plugins", "DLCQuestipelago", "ArchipelagoConnectionInfo.json");
        public const string SaveDirectory = "Saves";
        public static readonly string SaveFile = Path.Combine(SaveDirectory, "ArchipelagoSaveState.json");
    }
}
