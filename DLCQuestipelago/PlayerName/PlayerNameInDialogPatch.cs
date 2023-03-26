using System;
using System.Linq;
using BepInEx.Logging;
using DLCLib.HUD;
using DLCLib.Render;
using DLCQuestipelago.Archipelago;
using HarmonyLib;

namespace DLCQuestipelago.PlayerName
{
    [HarmonyPatch(typeof(DialogDisplay))]
    [HarmonyPatch(nameof(DialogDisplay.AddDialog))]
    public static class PlayerNameInDialogPatch
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

        //public void AddDialog(string name,string message,Animation animation,bool isSameSpeaker,bool forceLeftDialog)
        public static bool Prefix(DialogDisplay __instance, ref string name, ref string message, Animation animation, bool isSameSpeaker, bool forceLeftDialog)
        {
            name = _nameChanger.ChangePlayerNameInString(name);
            message = _nameChanger.ChangePlayerNameInString(message);
            return true; // run original logic
        }
    }
}
