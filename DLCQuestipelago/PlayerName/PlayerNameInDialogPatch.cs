using System;
using System.Diagnostics;
using DLCLib.HUD;
using DLCLib.Render;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.PlayerName
{
    [HarmonyPatch(typeof(DialogDisplay))]
    [HarmonyPatch(nameof(DialogDisplay.AddDialog))]
    public static class PlayerNameInDialogPatch
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

        //public void AddDialog(string name,string message,Animation animation,bool isSameSpeaker,bool forceLeftDialog)
        public static bool Prefix(DialogDisplay __instance, ref string name, ref string message, Animation animation, bool isSameSpeaker, bool forceLeftDialog)
        {
            try
            {
                name = _nameChanger.ChangePlayerNameInString(name);
                message = _nameChanger.ChangePlayerNameInString(message);
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PlayerNameInDialogPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
