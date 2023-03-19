using BepInEx.Logging;
using DLCLib.HUD;
using DLCLib.Render;
using DLCQuestipelago.Archipelago;
using HarmonyLib;

namespace DLCQuestipelago.PlayerName
{
    [HarmonyPatch(typeof(DialogDisplay))]
    [HarmonyPatch(nameof(DialogDisplay.AddDialog))]
    public static class PlayerToSlotNamePatch
    {
        private const string vanilla_name = "Player";
        private const string vanilla_name_changed = "xXx~P14y3R[69]SN1PA{117}~xXx";
        public const string NAME_CHANGED_PREFIX = "xXx~";
        public const string NAME_CHANGED_SUFFIX = "[69]4RCH1P374G0{117}~xXx";

        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        //public void AddDialog(string name,string message,Animation animation,bool isSameSpeaker,bool forceLeftDialog)
        public static bool Prefix(DialogDisplay __instance, ref string name, ref string message, Animation animation, bool isSameSpeaker, bool forceLeftDialog)
        {
            name = ChangePlayerNameInString(name);
            message = ChangePlayerNameInString(message);
            return true;
        }

        public static string ChangePlayerNameInString(string text)
        {
            var containsDefaultName = text.Contains(vanilla_name);
            var containsChangedName = text.Contains(vanilla_name_changed);
            if (!containsDefaultName && !containsChangedName)
            {
                return text;
            }

            var slotName = _archipelago.SlotData.SlotName;
            var apName = _archipelago.GetPlayerAlias(slotName) ?? slotName;

            if (containsChangedName)
            {
                if (!apName.StartsWith(NAME_CHANGED_PREFIX) || !apName.EndsWith(NAME_CHANGED_SUFFIX))
                {
                    apName = ChangeName(apName);
                }
                else
                {
                    apName = TurnLeet(apName);
                }

                return text.Replace(vanilla_name_changed, apName);
            }

            if (containsDefaultName)
            {
                return text.Replace(vanilla_name, apName);
            }

            return text;
        }

        public static string ChangeName(string name)
        {
            return $"{NAME_CHANGED_PREFIX}{TurnLeet(name)}{NAME_CHANGED_SUFFIX}";
        }

        public static string TurnLeet(string text)
        {
            return text
                .Replace("a", "4")
                .Replace("A", "4")
                .Replace("e", "3")
                .Replace("E", "3")
                .Replace("L", "7")
                .Replace("l", "1")
                .Replace("i", "1")
                .Replace("I", "1")
                .Replace("o", "0")
                .Replace("O", "0");
        }
    }
}
