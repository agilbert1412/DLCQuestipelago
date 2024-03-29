using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.HUD;
using DLCLib.Render;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Textures;
using HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DLCQuestipelago.Coins
{
    public static class CoinsAppearancePatch
    {
        private static Texture2D _brokenCoinTexture;
        private static Texture2D _coinPileTexture;
        private static FieldInfo _coinIconField;
        private static FieldInfo _idleAnimField;
        private static FieldInfo _animPlayerField;

        public static void ReplaceBrokenCoins(ManualLogSource log, ArchipelagoClient archipelago)
        {
            if (archipelago.SlotData.Coinsanity == Coinsanity.None || archipelago.SlotData.CoinBundleSize > 0)
            {
                return;
            }

            _brokenCoinTexture = TexturesLoader.GetTexture(log, Path.Combine("Coins", "cracked.png"));
            _coinPileTexture = TexturesLoader.GetTexture(log, Path.Combine("Coins", "pile.png"));

            // protected HUDIcon coinIcon;
            _coinIconField = typeof(CoinDisplay).GetField("coinIcon", BindingFlags.NonPublic | BindingFlags.Instance);

            // protected Animation idleAnim;
            _idleAnimField = typeof(Coin).GetField("idleAnim", BindingFlags.NonPublic | BindingFlags.Instance);

            // protected AnimationPlayer animPlayer;
            _animPlayerField = typeof(Coin).GetField("animPlayer", BindingFlags.NonPublic | BindingFlags.Instance);

            var currentScene = ReplaceCoinDisplay();

            foreach (var coin in currentScene.Coins)
            {
                ReplaceForBrokenAnimation(coin);
            }
        }

        private static Scene ReplaceCoinDisplay()
        {
            var currentScene = Singleton<SceneManager>.Instance.CurrentScene;
            var coinDisplay = currentScene.HUDManager.CoinDisplay;
            var coinIcon = (HUDIcon)_coinIconField.GetValue(coinDisplay);
            coinIcon.Texture = _coinPileTexture;
            coinIcon.SourceRectangle = new Rectangle(0, 0, 64, 64);
            return currentScene;
        }

        // public override void LoadContent()
        private static void ReplaceForBrokenAnimation(Coin coin)
        {
            // var objectSpriteSheet = Singleton<SceneManager>.Instance.CurrentScene.AssetManager.BaseObjectSpriteSheet;
            // objectSpriteSheet.Texture.SaveAsPng(new FileStream("BaseObjectSpriteSheet.png", FileMode.Create), objectSpriteSheet.Texture.Width, objectSpriteSheet.Texture.Height);
            var animation = new Animation(_brokenCoinTexture, new List<Rectangle>()
            {
                new Rectangle(0, 0, 64, 64),
                new Rectangle(0, 0, 64, 64),
            }, 1f);
            _idleAnimField.SetValue(coin, animation);
            var scale = new Vector2(Units.MetersPerPixelVector.X * 0.25f, Units.MetersPerPixelVector.Y * 0.25f);
            var animPlayer = new ResizedAnimationPlayer(scale);
            _animPlayerField.SetValue(coin, animPlayer);
            var idleAnim = (Animation)_idleAnimField.GetValue(coin);
            animPlayer.Play(idleAnim);
        }
    }
}
