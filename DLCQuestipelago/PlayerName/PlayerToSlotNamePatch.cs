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
    public static class PlayerToSlotNamePatch
    {
        private const int MAX_ALIAS_LENGTH = 16;
        private const string vanilla_name = "Player";
        private const string vanilla_name_changed = "xXx~P14y3R[69]SN1PA{117}~xXx";
        private static readonly string[] _decorators = new[] {"x", "X", "x~", "X~", "xx", "XX", "Xx", "xX", "xx~", "XX~", "Xx~", "xX~", "xXx", "XxX", "xXx~", "XxX~" };

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
            var validDecorators = _decorators.Where(x => (x.Length * 2) + name.Length <= MAX_ALIAS_LENGTH).OrderBy(x => x.Length).ToArray();
            var chosenDecorator = "";
            if (validDecorators.Any())
            {
                var longValidDecorators = validDecorators.Where(x => x.Length == validDecorators.Last().Length).ToArray();
                var random = new Random(int.Parse(_archipelago.SlotData.Seed));
                var chosenIndex = random.Next(0, longValidDecorators.Length);
                chosenDecorator = longValidDecorators[chosenIndex];
            }

            var prefixDecorator = chosenDecorator;
            var suffixDecorator = new string(chosenDecorator.Reverse().ToArray());
            return $"{prefixDecorator}{TurnLeet(name)}{suffixDecorator}";
        }

        public static string TurnLeet(string text)
        {
            return text
                .Replace("l", "1")
                .ToUpper()
                .Replace("A", "4")
                .Replace("E", "3")
                .Replace("L", "7")
                .Replace("I", "1")
                .Replace("O", "0");
        }
    }
}
