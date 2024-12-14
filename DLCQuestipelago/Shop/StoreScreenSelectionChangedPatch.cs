﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Core;
using DLCDataTypes;
using DLCLib;
using DLCLib.HUD;
using DLCLib.Screens;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Items;
using DLCQuestipelago.Textures;
using HarmonyLib;
using HUD;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DLCQuestipelago.Shop
{
    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("OnSelectionChanged")]
    public static class StoreScreenSelectionChangedPatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;
        private static Texture2D _coinPileTexture;

        private static Dictionary<string, string> _canadianDictionary;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _coinPileTexture = TexturesLoader.GetTexture(_logger, Path.Combine("Coins", "pile.png"));

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

        // protected override void OnSelectionChanged(bool changedByMouse, bool playSound)
        private static void Postfix(StoreScreen __instance, bool changedByMouse, bool playSound)
        {
            try
            {
                // protected DLCDetailPanel detailPanel;
                var detailPanelField = typeof(StoreScreen).GetField("detailPanel", BindingFlags.NonPublic | BindingFlags.Instance);
                var detailPanel = (DLCDetailPanel)detailPanelField.GetValue(__instance);

                // protected int currentSelectionIndex;
                var currentIndexField = typeof(StoreScreen).GetField("currentSelectionIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                var currentIndex = (int)currentIndexField.GetValue(__instance);

                // protected List<DLCMenuEntry> dlcMenuEntries;
                var dlcMenuEntriesField = typeof(StoreScreen).GetField("dlcMenuEntries", BindingFlags.NonPublic | BindingFlags.Instance);
                var menuEntries = (List<DLCMenuEntry>)dlcMenuEntriesField.GetValue(__instance);
                if (currentIndex >= menuEntries.Count)
                {
                    return;
                }

                var selectedDLCData = (menuEntries[currentIndex] as DLCPackMenuEntry)?.Pack?.Data;
                var selectedDLCName = selectedDLCData?.DisplayName;

                UpdateInventoryAndPrice(detailPanel, selectedDLCData);

                ScoutCurrentDLC(menuEntries, selectedDLCName, detailPanel);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(StoreScreenSelectionChangedPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }

        private static void ScoutCurrentDLC(List<DLCMenuEntry> menuEntries, string selectedDLCName, DLCDetailPanel detailPanel)
        {
            var dlcMenuEntries = menuEntries.OfType<DLCPackMenuEntry>();
            var dlcNames = dlcMenuEntries.Select(x => x.Pack.Data.DisplayName);
            var scoutedDLCs = _archipelago.ScoutManyLocations(dlcNames);
            if (scoutedDLCs == null || selectedDLCName == null || !scoutedDLCs.ContainsKey(selectedDLCName))
            {
                return;
            }

            var scoutedDLC = scoutedDLCs[selectedDLCName];
            var itemName = scoutedDLC.ItemName;
            var ownerName = scoutedDLC.PlayerName;
            var hasCanadianPack = _archipelago.HasReceivedItem("Canadian Dialog Pack");
            var canadianSuffix = hasCanadianPack ? ", sorry" : "";
            var refundsSuffix = $"There are no refunds for this item{canadianSuffix}.";
            var description = $"{ownerName}'s {itemName}\n{refundsSuffix}";
            description = hasCanadianPack ? TurnCanadian(description) : description;
            var descriptionTextField = typeof(DLCDetailPanel).GetField("descriptionText", BindingFlags.NonPublic | BindingFlags.Instance);
            var descriptionText = (HUDText)descriptionTextField.GetValue(detailPanel);

            descriptionText.ClearString();
            var font = descriptionText.Font;
            var sizeX = descriptionText.Size.X;
            descriptionText.Append(TextUtil.FitStringToWidth(font, description, sizeX));
            return;
        }

        private static void UpdateInventoryAndPrice(DLCDetailPanel detailPanel, DLCPackData selectedDLCData)
        {
            // protected bool useBossCoins;
            var useBossCoinsField = typeof(DLCDetailPanel).GetField("useBossCoins", BindingFlags.NonPublic | BindingFlags.Instance);
            var useBossCoins = (bool)useBossCoinsField.GetValue(detailPanel);

            // protected HUDText priceCoinAmountText;
            var priceCoinAmountTextField = typeof(DLCDetailPanel).GetField("priceCoinAmountText", BindingFlags.NonPublic | BindingFlags.Instance);
            var priceCoinAmountText = (HUDText)priceCoinAmountTextField.GetValue(detailPanel);

            // protected HUDText playerCoinAmountText;
            var playerCoinAmountTextField = typeof(DLCDetailPanel).GetField("playerCoinAmountText", BindingFlags.NonPublic | BindingFlags.Instance);
            var playerCoinAmountText = (HUDText)playerCoinAmountTextField.GetValue(detailPanel);

            var numberCoins = useBossCoins
                ? Singleton<SceneManager>.Instance.CurrentScene.Player.Inventory.BossCoins
                : CoinsanityUtils.GetCurrentCoins(_archipelago);
            numberCoins = Math.Round(numberCoins, 2);

            if (numberCoins >= selectedDLCData.Cost)
            {
                priceCoinAmountText.Tint = ColorUtil.PriceCanAffordText;
            }
            else
            {
                priceCoinAmountText.Tint = ColorUtil.PriceCannotAffordText;
            }

            playerCoinAmountText.ClearString();
            playerCoinAmountText.Append(numberCoins.ToString());

            if (_archipelago.SlotData.Coinsanity == Coinsanity.None || _archipelago.SlotData.CoinBundleSize > 0 || useBossCoins)
            {
                return;
            }

            // protected HUDIcon playerCoinIcon;
            var playerCoinIconField = typeof(DLCDetailPanel).GetField("playerCoinIcon", BindingFlags.NonPublic | BindingFlags.Instance);
            var playerCoinIcon = (HUDIcon)playerCoinIconField.GetValue(detailPanel);
            playerCoinIcon.Texture = _coinPileTexture;
            playerCoinIcon.SourceRectangle = new Rectangle(0, 0, 64, 64);
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
