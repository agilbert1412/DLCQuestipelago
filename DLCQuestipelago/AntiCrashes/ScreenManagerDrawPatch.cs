using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using GameStateManagement;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.AntiCrashes
{
    [HarmonyPatch(typeof(ScreenManager))]
    [HarmonyPatch(nameof(ScreenManager.Draw))]
    public static class ScreenManagerDrawPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public override void Draw(GameTime gameTime)
        private static bool Prefix(ScreenManager __instance, GameTime gameTime)
        {
            try
            {
                var screensField = typeof(ScreenManager).GetField("screens", BindingFlags.NonPublic | BindingFlags.Instance);
                var screens = (List<GameScreen>)screensField.GetValue(__instance);
                screens = screens.Where(screen => screen.ScreenState != ScreenState.Hidden).ToList();
                foreach (var screen in screens)
                {
                    screen.Draw(gameTime);
                }
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ScreenManagerDrawPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return false; // don't run original logic
            }
        }
    }
}
