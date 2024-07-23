using System;
using System.IO;
using GameStateManagement;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework.Graphics;

namespace DLCQuestipelago.Textures
{
    public class TexturesLoader
    {
        public static Texture2D GetTexture(ILogger logger, string texture)
        {
            var graphicsDevice = ScreenManager.Instance.GraphicsDevice;
            if (graphicsDevice == null)
            {
                throw new InvalidOperationException("No Graphics Device Loaded");
            }

            var currentTexturesFolder = Path.Combine("BepInEx", "plugins", "DLCQuestipelago", "Textures");
            if (!Directory.Exists(currentTexturesFolder))
            {
                throw new InvalidOperationException("Could not find Textures folder");
            }
            
            var relativePathToTexture = Path.Combine(currentTexturesFolder, texture);
            if (!File.Exists(relativePathToTexture))
            {
                throw new InvalidOperationException($"Tried to load texture '{relativePathToTexture}', but it couldn't be found!");
            }

            logger.LogInfo($"Loading Texture file '{relativePathToTexture}'");
            using var stream = new FileStream(relativePathToTexture, FileMode.Open);
            return Texture2D.FromStream(graphicsDevice, stream);
        }
    }
}
