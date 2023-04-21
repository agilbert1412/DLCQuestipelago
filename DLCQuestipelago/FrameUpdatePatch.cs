using System;
using System.Diagnostics;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(DLCGame.DLCGame))]
    [HarmonyPatch("Update")]
    public static class FrameUpdatePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(FrameUpdatePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
