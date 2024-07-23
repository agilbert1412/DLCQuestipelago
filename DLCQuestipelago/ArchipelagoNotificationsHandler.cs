using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DLCDataTypes;
using DLCLib;
using DLCLib.DLC;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.DualContentManager;
using DLCQuestipelago.Items;
using DLCQuestipelago.Textures;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Notifications;
using SpriteSheetRuntime;

namespace DLCQuestipelago
{
    public class ArchipelagoNotificationsHandler
    {
        private ILogger _logger;
        private DLCQArchipelagoClient _archipelago;
        private DLCDualContentManager _dualContentManager;
        private DLCDualAssetManager _dualAssetManager;
        private List<DLCPack> _dlcPacks;
        private SpriteSheet spriteSheet => SceneManager.Instance.CurrentScene.HUDManager.SpriteSheet;
        private Texture2D spriteSheetTexture => spriteSheet.Texture;
        private Texture2D coinPieceTexture;
        private Texture2D coinPileTexture;

        public ArchipelagoNotificationsHandler(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        public void InitializeTextures()
        {
            coinPieceTexture = TexturesLoader.GetTexture(_logger, Path.Combine("Coins", "piece.png"));
            coinPileTexture = TexturesLoader.GetTexture(_logger, Path.Combine("Coins", "pile.png"));
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

        public void AddItemNotification(string itemName)
        {
            var isCoin = itemName is CoinsanityUtils.BASIC_CAMPAIGN_COIN_NAME
                or CoinsanityUtils.LFOD_CAMPAIGN_COIN_NAME
                or CoinsanityUtils.BASIC_CAMPAIGN_COIN_PIECE_NAME
                or CoinsanityUtils.LFOD_CAMPAIGN_COIN_PIECE_NAME;
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
            var campaign = itemName.Split(':')[0];
            var coinBundleSize = _archipelago.SlotData.CoinBundleSize;

            var notificationToChange = GetExistingNotificationForCoins(campaign, coinBundleSize);

            if (notificationToChange != null)
            {
                ModifyExistingCoinNotification(notificationToChange, coinBundleSize, campaign);
                return;
            }

            CreateNewCoinNotification(coinBundleSize, campaign);
        }

        private void ModifyExistingCoinNotification(Notification notificationToChange, int numCoinsPerBundle, string campaign)
        {
            var existingDescription = notificationToChange.Description;
            var indexOfCoin = existingDescription.IndexOf("Coin", StringComparison.InvariantCultureIgnoreCase);
            var firstPart = existingDescription.Substring(0, indexOfCoin);
            var words = firstPart.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var numCoins = int.Parse(words[words.Length - 1]);
            if (numCoinsPerBundle <= 0)
            {
                numCoins += 1;
                notificationToChange.Texture = coinPileTexture;
                notificationToChange.Description = GetNotificationDescriptionForNumberOfCoinPieces(numCoins, campaign);
            }
            else
            {
                numCoins += numCoinsPerBundle;
                notificationToChange.Description = GetNotificationDescriptionForNumberOfCoins(numCoins, campaign);
            }
        }

        private void CreateNewCoinNotification(int numCoins, string campaign)
        {
            // texture.SaveAsPng(new FileStream("Coin.png", FileMode.Create), texture.Width, texture.Height);
            if (numCoins <= 0)
            {
                var textureRectangle = new Rectangle(0, 0, 64, 64);
                var description = GetNotificationDescriptionForNumberOfCoinPieces(1, campaign);
                AddNotification(description, coinPieceTexture, textureRectangle);
            }
            else
            {
                var textureRectangle = spriteSheet.SourceRectangle("hud_coin");
                var description = GetNotificationDescriptionForNumberOfCoins(numCoins, campaign);
                AddNotification(description, spriteSheetTexture, textureRectangle);
            }
        }

        private static string GetNotificationDescriptionForNumberOfCoinPieces(int numCoinPieces, string campaign)
        {
            return GetNotificationDescriptionForNumericItem(campaign, "Coin Piece", numCoinPieces);
        }

        private static string GetNotificationDescriptionForNumberOfCoins(int numCoins, string campaign)
        {
            return GetNotificationDescriptionForNumericItem(campaign, "Coin", numCoins);
        }

        private static string GetNotificationDescriptionForNumericItem(string campaign, string item, int number)
        {
            const string pattern = "{0}: {1} {2}{3}";
            var pluralModifier = number > 1 ? "s" : "";
            var description = string.Format(pattern, campaign, number, item, pluralModifier);
            return description;
        }

        private static Notification GetExistingNotificationForCoins(string campaign, int coinBundleSize)
        {
            var notificationsField = typeof(NotificationManager).GetField("notifications", BindingFlags.NonPublic | BindingFlags.Instance);
            var notifications = (Queue<Notification>)notificationsField.GetValue(NotificationManager.Instance);

            if (notifications.Count < 2)
            {
                return null;
            }
            
            foreach (var existingNotification in notifications.Skip(1))
            {
                var existingDescription = existingNotification.Description;
                var validEndings = new[] { "Coin", "Coin Piece" };
                var isCoinNotification = validEndings.Any(x => existingDescription.EndsWith(x) || existingDescription.EndsWith($"{x}s"));
                if (existingDescription.StartsWith(campaign) && isCoinNotification)
                {
                    return existingNotification;
                }
            }

            return null;
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

        private void AddNotification(string description, Texture2D texture, Rectangle textureRectangle)
        {
            var newNotification = CreateNewItemNotification(description, texture, textureRectangle);
            NotificationManager.Instance.AddNotification(newNotification);
        }

        private Notification CreateNewItemNotification(string description, Texture2D texture, Rectangle textureRectangle)
        {
            return new Notification()
            {
                Title = "New Archipelago Item Received!",
                Description = description,
                Texture = texture,
                SourceRectangle = textureRectangle,
                Tint = Color.White,
                CueName = "toast_up"
            };
        }

        public void AddGiftNotification(string giftName, bool isTrap)
        {
            var iconPackName = isTrap ? "The Zombie Pack" : "Double Jump Pack";
            var dlcPack = _dlcPacks.Where(x => x.Data.DisplayName == iconPackName).ToArray();
            var iconName = dlcPack.First().Data.IconName;
            _dualAssetManager.FindIcon(iconName, out var texture, out var rectangle);
            AddGiftNotification(giftName, texture, rectangle, isTrap);
        }

        private void AddGiftNotification(string description, Texture2D texture, Rectangle icon, bool isTrap)
        {
            var newNotification = CreateGiftNotification(description, texture, icon, isTrap);
            NotificationManager.Instance.AddNotification(newNotification);
        }

        private Notification CreateGiftNotification(string giftName, Texture2D texture, Rectangle icon, bool isTrap)
        {
            var itemName = isTrap ? "Trap" : "Gift";
            return new Notification()
            {
                Title = $"New {itemName} Received!",
                Description = giftName,
                Texture = texture,
                SourceRectangle = icon,
                Tint = Color.White,
                CueName = "toast_up"
            };
        }
    }
}