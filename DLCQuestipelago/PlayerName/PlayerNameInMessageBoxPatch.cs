using System;
using System.Diagnostics;
using GameStateManagement;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.PlayerName
{
    [HarmonyPatch(typeof(MessageBoxScreen))]
    [HarmonyPatch(MethodType.Constructor, typeof(string), typeof(bool))]
    public static class PlayerNameInMessageBoxPatch
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static NameChanger _nameChanger;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, NameChanger nameChanger)
        {
            _logger = logger;
            _archipelago = archipelago;
            _nameChanger = nameChanger;
        }

        //public MessageBoxScreen(string message, bool includeUsageText)
        public static bool Prefix(MessageBoxScreen __instance, ref string message, bool includeUsageText)
        {
            try
            {
                if (_nameChanger == null)
                {
                    return true; // run original logic
                }

                message = _nameChanger.ChangePlayerNameInString(message);
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlayerNameInMessageBoxPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
