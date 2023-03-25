using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Core;
using DLCDataTypes;
using DLCLib;
using DLCLib.DLC;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.DualContentManager;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Notifications;

namespace DLCQuestipelago
{
    public class ArchipelagoNotificationsHandler
    {
        private ManualLogSource _log;
        private ArchipelagoClient _archipelago;
        private DLCDualContentManager _dualContentManager;
        private DLCDualAssetManager _dualAssetManager;
        private List<DLCPack> _dlcPacks;

        public ArchipelagoNotificationsHandler(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        public void LoadDlcPacks(DLCDualContentManager dualContentManager, DLCDualAssetManager dualAssetManager)
        {
            _dualContentManager = dualContentManager;
            _dualAssetManager = dualAssetManager;
            LoadAllDlcPacks();
        }

        private void LoadAllDlcPacks()
        {
            var dlcManifest = _dualContentManager.Load<List<string>>("manifest", ContentSource.DlcCampaign);
            var lfodManifest = _dualContentManager.Load<List<string>>("manifest", ContentSource.LfodCampaign);
            var path = Path.Combine("data", "dlc");
            var allDlcManifest = dlcManifest.Union(lfodManifest).Where(file => Path.GetDirectoryName(file).Equals(path));
            _dlcPacks = new List<DLCPack>();
            foreach (string assetName in allDlcManifest)
            {
                var packData = _dualContentManager.Load<DLCPackData>(assetName);
                var pack = new DLCPack(packData);
                _dlcPacks.Add(pack);
            }
        }

        public void AddNotification(string itemName)
        {
            var isCoin = itemName is InventoryCoinsGetPatch.BASIC_CAMPAIGN_COIN_NAME
                or InventoryCoinsGetPatch.LFOD_CAMPAIGN_COIN_NAME;
            if (isCoin)
            {
                AddCoinNotification(itemName);
            }
            else
            {
                AddDLCNotification(itemName);
            }
        }

        private void AddCoinNotification(string itemName)
        {
            const string pattern = "{0}: {1} Coin{2}";
            var campaign = itemName.Split(':')[0];
            var notificationsField = typeof(NotificationManager).GetField("notifications", BindingFlags.NonPublic | BindingFlags.Instance);
            var notifications = (Queue<Notification>)notificationsField.GetValue(NotificationManager.Instance);

            var numCoinsPerBundle = _archipelago.SlotData.CoinBundleSize;
            var numCoins = numCoinsPerBundle;

            Notification notificationToChange = null;
            foreach (var existingNotification in notifications.Skip(1))
            {
                var existingDescription = existingNotification.Description;
                var endsWithCoin = existingDescription.EndsWith("Coin") || existingDescription.EndsWith("Coins");
                if (existingDescription.StartsWith(campaign) && endsWithCoin)
                {
                    notificationToChange = existingNotification;
                    var words = existingDescription.Split(' ');
                    numCoins = int.Parse(words[words.Length - 2]) + numCoinsPerBundle;
                    break;
                }
            }

            var pluralModifier = numCoins > 1 ? "s" : "";
            var description = string.Format(pattern, campaign, numCoins, pluralModifier);

            if (notificationToChange != null)
            {
                notificationToChange.Description = description;
                return;
            }

            var spriteSheet = SceneManager.Instance.CurrentScene.HUDManager.SpriteSheet;
            var texture = spriteSheet.Texture;
            var icon = spriteSheet.SourceRectangle("hud_coin");

            AddNotification(description, texture, icon);
        }

        private bool AddDLCNotification(string dlcName)
        {
            var receivedDLCPack = _dlcPacks.Where(x => x.Data.DisplayName == dlcName).ToArray();

            if (!receivedDLCPack.Any())
            {
                return false;
            }

            var iconName = receivedDLCPack.First().Data.IconName;
            _dualAssetManager.FindIcon(iconName, out var texture, out var rectangle);

            AddNotification(dlcName, texture, rectangle);
            return true;
        }

        private void AddNotification(string description, Texture2D texture, Rectangle icon)
        {
            var newNotification = CreateNotification(description, texture, icon);
            NotificationManager.Instance.AddNotification(newNotification);
        }

        private Notification CreateNotification(string description, Texture2D texture, Rectangle icon)
        {
            return new Notification()
            {
                Title = "New Archipelago Item Received!",
                Description = description,
                Texture = texture,
                SourceRectangle = icon,
                Tint = Color.White,
                CueName = "toast_up"
            };
        }
    }
}