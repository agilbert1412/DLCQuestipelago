using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.DLC;
using DLCLib.HUD;
using DLCLib.Screens;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using HarmonyLib;
using HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(StoreScreen))]
    [HarmonyPatch("SetupEntries")]
    public static class StoreScreenSetupEntriesPatch
    {
        private static ManualLogSource _log;
        private static LocationChecker _locationChecker;
        private static ItemParser _itemParser;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker, ItemParser itemParser)
        {
            _log = log;
            _locationChecker = locationChecker;
            _itemParser = itemParser;
        }
        
        public static bool Prefix(StoreScreen __instance)
        {
            var dlcSpriteSheet = SceneManager.Instance.CurrentScene.AssetManager.DLCSpriteSheet;

            var scrollContainerField = typeof(StoreScreen).GetField("scrollContainer", BindingFlags.NonPublic | BindingFlags.Instance);
            var scrollContainer = (HUDStackContainer)scrollContainerField.GetValue(__instance);
            var dlcMenuEntriesField = typeof(StoreScreen).GetField("dlcMenuEntries", BindingFlags.NonPublic | BindingFlags.Instance);
            var dlcMenuEntries = (List<DLCMenuEntry>)dlcMenuEntriesField.GetValue(__instance);
            var modeField = typeof(StoreScreen).GetField("Mode", BindingFlags.NonPublic | BindingFlags.Instance);
            var mode = (StoreScreen.StoreModeEnum)modeField.GetValue(__instance);
            var containerField = typeof(StoreScreen).GetField("container", BindingFlags.NonPublic | BindingFlags.Instance);
            var container = (HUDContainer)containerField.GetValue(__instance);

            scrollContainer.ClearElements();
            dlcMenuEntries.Clear();
            AddCorrectDLCsToStore(__instance, mode, dlcSpriteSheet, dlcMenuEntries);

            SetupTabColors(__instance, mode);
            SetupNoDLCAvailableText(__instance, dlcMenuEntries, mode);
            container.Layout();
            return false; // don't run original logic
        }

        private static void AddCorrectDLCsToStore(StoreScreen storeScreen, StoreScreen.StoreModeEnum mode,
            SpriteSheet dlcSpriteSheet, List<DLCMenuEntry> dlcMenuEntries)
        {
            var showBossDLCOnlyField = typeof(StoreScreen).GetField("showBossDLCOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            var showBossDLCOnly = (bool)showBossDLCOnlyField.GetValue(storeScreen);
            var fontField = typeof(StoreScreen).GetField("font", BindingFlags.NonPublic | BindingFlags.Instance);
            var font = (SpriteFont)fontField.GetValue(storeScreen);
            var addEntryMethod = typeof(StoreScreen).GetMethod("AddEntry", BindingFlags.NonPublic | BindingFlags.Instance);

            var menuOrder = 0;
            var availableMode = mode == StoreScreen.StoreModeEnum.AvailableDLC;
            var purchasedMode = mode == StoreScreen.StoreModeEnum.PurchasedDLC;
            foreach (var pack in Singleton<DLCManager>.Instance.Packs.Values)
            {
                if (availableMode && pack.Data.IsBossDLC != showBossDLCOnly)
                {
                    continue;
                }

                /*if ((purchasedMode || pack.State == DLCPackStateEnum.Visible) &&
                    (purchasedMode || pack.Data.IsBossDLC == showBossDLCOnly) &&
                    (availableMode || pack.State == DLCPackStateEnum.Purchased))*/
                if (purchasedMode && _locationChecker.IsLocationChecked(pack.Data.DisplayName) ||
                    (availableMode && _locationChecker.IsLocationMissing(pack.Data.DisplayName) && pack.State != DLCPackStateEnum.Locked) ||
                    (availableMode && pack.Data.IsBossDLC && pack.State != DLCPackStateEnum.Purchased))
                {
                    var entry = new DLCPackMenuEntry(font, dlcSpriteSheet, pack, menuOrder);
                    entry.ParentScreen = storeScreen;
                    if (availableMode)
                    {
                        entry.Selected += storeScreen.PurchaseSelectedPack;
                    }
                    else if (purchasedMode && _locationChecker.IsLocationChecked(pack.Data.DisplayName) && _itemParser.ReceivedDLCs.Contains(pack.Data.Name))
                    {
                        entry.Selected += storeScreen.ToggleSelectedPack;
                    }

                    dlcMenuEntries.Add(entry);
                    addEntryMethod.Invoke(storeScreen, new[] { (DLCMenuEntry)entry });
                    ++menuOrder;
                }
            }
        }

        private static void SetupTabColors(StoreScreen storeScreen, StoreScreen.StoreModeEnum mode)
        {
            var availableTabTextField = typeof(StoreScreen).GetField("availableTabText", BindingFlags.NonPublic | BindingFlags.Instance);
            var availableTabText = (HUDText)availableTabTextField.GetValue(storeScreen);
            var purchasedTabTextField = typeof(StoreScreen).GetField("purchasedTabText", BindingFlags.NonPublic | BindingFlags.Instance);
            var purchasedTabText = (HUDText)purchasedTabTextField.GetValue(storeScreen);

            switch (mode)
            {
                case StoreScreen.StoreModeEnum.AvailableDLC:
                    availableTabText.BackgroundColor = ColorUtil.StoreTabSelected;
                    availableTabText.Tint = ColorUtil.DLCPackNameSelectedText;
                    availableTabText.TextDropShadowColor = ColorUtil.DLCPackNameSelectedTextShadow;
                    purchasedTabText.BackgroundColor = ColorUtil.StoreTabUnselected;
                    purchasedTabText.Tint = ColorUtil.DLCPackNameUnselectedText;
                    purchasedTabText.TextDropShadowColor = ColorUtil.DLCPackNameUnselectedTextShadow;
                    break;
                case StoreScreen.StoreModeEnum.PurchasedDLC:
                    availableTabText.BackgroundColor = ColorUtil.StoreTabUnselected;
                    availableTabText.Tint = ColorUtil.DLCPackNameUnselectedText;
                    availableTabText.TextDropShadowColor = ColorUtil.DLCPackNameUnselectedTextShadow;
                    purchasedTabText.BackgroundColor = ColorUtil.StoreTabSelected;
                    purchasedTabText.Tint = ColorUtil.DLCPackNameSelectedText;
                    purchasedTabText.TextDropShadowColor = ColorUtil.DLCPackNameSelectedTextShadow;
                    break;
            }
        }

        private static void SetupNoDLCAvailableText(StoreScreen storeScreen, List<DLCMenuEntry> dlcMenuEntries, StoreScreen.StoreModeEnum mode)
        {
            var noDLCAvailableTextField = typeof(StoreScreen).GetField("noDLCAvailableText", BindingFlags.NonPublic | BindingFlags.Instance);
            var noDLCAvailableText = (HUDText)noDLCAvailableTextField.GetValue(storeScreen);

            if (dlcMenuEntries.Count == 0)
            {
                noDLCAvailableText.Visible = true;
                noDLCAvailableText.ClearString();
                if (mode == StoreScreen.StoreModeEnum.AvailableDLC)
                {
                    noDLCAvailableText.Append("No DLC Available");
                }
                else
                {
                    noDLCAvailableText.Append("No DLC Purchased");
                }

                noDLCAvailableText.SetSize(noDLCAvailableText.MeasureString() + new Vector2(10f, 0.0f));
            }
            else
            {
                noDLCAvailableText.Visible = false;
            }
        }
    }
}
