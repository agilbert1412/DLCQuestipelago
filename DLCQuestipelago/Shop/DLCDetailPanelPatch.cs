using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.DLC;
using DLCLib.HUD;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Items;
using HarmonyLib;
using HUD;

namespace DLCQuestipelago.Shop
{
    [HarmonyPatch(typeof(DLCDetailPanel))]
    [HarmonyPatch(nameof(DLCDetailPanel.SetData))]
    public static class DLCDetailPanelPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        private static Dictionary<string, string> _canadianDictionary;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;

            InitializeCanadianDictionary();
        }

        private static void InitializeCanadianDictionary()
        {
            var canadianWords = new Dictionary<string, string>
            {
                { "color", "colour" },
                { "gray", "grey" },
                { "favorite", "favourite" },
                { "about", "aboot" },
                { "honor", "honour" },
                { "soda", "pop" },
                { "cigarette", "dart" },
                { "sofa", "chesterfield" },
                { "couch", "chesterfield" },
                { "hat", "toque" },
                { "beanie", "toque" },
                { "sweater", "bunnyhug" },
                { "hoodie", "bunnyhug" },
                { "bodega", "dep" },
                { "suck-up", "keener" },
                { "try-hard", "keener" },
                { "brown-noser", "keener" },
                { "coin", "loonie" },
                { "dollar", "buck" },
                { "kilometre", "click" },
                { "right?", "eh?" },
                { "what?", "eh?" },
                { "canadian", "canuck" },
                { "commotion", "kerfuffle" },
                { "flurry", "kerfuffle" },
                { "welfare", "pogey" },
                { "ass", "arse" },
                { "whitener", "creamer" },
                { "recitation", "tutorial" },
                { "electricity", "hydro" },
                { "faucet", "tap" },
                { "bathroom", "washroom" },
                { "check", "cheque" },
                { "center", "centre" },
                { "foot", "meter" },
                { "feet", "meters" },
                { "inch", "centimeter" },
                { "fahrenheit", "celsius" },
                { "pound", "kilogram" },
                { "21", "19" },
                { "gun", "health care" },
            };

            _canadianDictionary = new Dictionary<string, string>();
            foreach (var word in canadianWords.Keys.ToArray())
            {
                var canadianWord = canadianWords[word];
                var lowerWord = word.ToLower();
                var lowerCanadianWord = canadianWord.ToLower();
                var upperWord = word.ToUpper();
                var upperCanadianWord = canadianWord.ToUpper();
                var capitalizedWord = upperWord.Substring(0, 1) + lowerWord.Substring(1);
                var capitalizedCanadianWord = upperCanadianWord.Substring(0, 1) + lowerCanadianWord.Substring(1);
                TryAdd(_canadianDictionary, lowerWord, lowerCanadianWord);
                TryAdd(_canadianDictionary, upperWord, upperCanadianWord);
                TryAdd(_canadianDictionary, capitalizedWord, capitalizedCanadianWord);
            }
        }

        private static void TryAdd<T1, T2>(Dictionary<T1, T2> dic, T1 keyToAdd, T2 valueToAdd)
        {
            if (dic.ContainsKey(keyToAdd))
            {
                return;
            }

            dic.Add(keyToAdd, valueToAdd);
        }

        // public void SetData(DLCPack dlc)
        private static void Postfix(DLCDetailPanel __instance, DLCPack dlc)
        {
            try
            {
                var dlcName = dlc.Data.DisplayName;
                var scoutedLocation = _archipelago.ScoutSingleLocation(dlcName);
                if (scoutedLocation == null)
                {
                    return;
                }

                var itemName = scoutedLocation.ItemName;
                var ownerName = scoutedLocation.PlayerName;
                var hasCanadianPack = Singleton<DLCManager>.Instance.IsPurchased("canadian", false);
                var canadianSuffix = hasCanadianPack ? ", sorry" : "";
                var refundsSuffix = $" There are no refunds for this item{canadianSuffix}.";
                var description = $"{ownerName}'s {itemName}\n{refundsSuffix}";
                description = hasCanadianPack ? TurnCanadian(description) : description;
                var descriptionTextField = typeof(DLCDetailPanel).GetField("descriptionText", BindingFlags.NonPublic | BindingFlags.Instance);
                var descriptionText = (HUDText)descriptionTextField.GetValue(__instance);

                descriptionText.ClearString();
                var font = descriptionText.Font;
                var sizeX = descriptionText.Size.X;
                descriptionText.Append(TextUtil.FitStringToWidth(font, description, sizeX));
                return;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(DLCDetailPanelPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }

        private static string TurnCanadian(string text)
        {
            var canadianText = $"{text}";
            foreach (var normalWord in _canadianDictionary.Keys.ToArray())
            {
                if (!canadianText.Contains(normalWord))
                {
                    continue;
                }

                var canadianWord = _canadianDictionary[normalWord];
                canadianText = canadianText.Replace(normalWord, canadianWord);
            }

            return canadianText;
        }
    }
}
