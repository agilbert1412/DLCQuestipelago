using System;
using System.Diagnostics;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(DLCGame.DLCGame))]
    [HarmonyPatch("Update")]
    public static class FrameUpdatePatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // protected override void Update(GameTime gameTime)
        private static void Postfix(DLCGame.DLCGame __instance, GameTime gameTime)
        {
            try
            {
                Plugin.Instance.OnUpdateTicked(gameTime);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FrameUpdatePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
