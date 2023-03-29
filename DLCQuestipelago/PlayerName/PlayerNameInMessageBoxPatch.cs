using BepInEx.Logging;
using DLCQuestipelago.Archipelago;
using GameStateManagement;
using HarmonyLib;

namespace DLCQuestipelago.PlayerName
{
    [HarmonyPatch(typeof(MessageBoxScreen))]
    [HarmonyPatch(MethodType.Constructor, typeof(string), typeof(bool))]
    public static class PlayerNameInMessageBoxPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static NameChanger _nameChanger;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, NameChanger nameChanger)
        {
            _log = log;
            _archipelago = archipelago;
            _nameChanger = nameChanger;
        }

        //public MessageBoxScreen(string message, bool includeUsageText)
        public static bool Prefix(MessageBoxScreen __instance, ref string message, bool includeUsageText)
        {
            if (_nameChanger == null)
            {
                return true; // run original logic
            }

            message = _nameChanger.ChangePlayerNameInString(message);
            return true; // run original logic
        }
    }
}
