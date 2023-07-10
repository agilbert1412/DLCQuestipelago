using System;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using Core.Audio;
using DLCLib;
using DLCLib.Effects;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DLCQuestipelago.AntiCrashes
{
    [HarmonyPatch(typeof(Scene))]
    [HarmonyPatch(nameof(Scene.Draw))]
    public static class DrawScenePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        private static bool Prefix(Scene __instance, GameTime gameTime, SpriteBatch spriteBatch)
        {
            try
            {
                var effectsManagerField = typeof(Scene).GetField("effectsManager", BindingFlags.NonPublic | BindingFlags.Instance);
                var pointSamplerStateField = typeof(Scene).GetField("pointSamplerState", BindingFlags.NonPublic | BindingFlags.Instance);

                var effectsManager = (EffectsManager)effectsManagerField.GetValue(__instance);
                var pointSamplerState = (SamplerState)pointSamplerStateField.GetValue(__instance);

                bool flag1 = effectsManager.IsSwirlStarting();
                bool flag2 = !effectsManager.IsSwirlActive();
                if (flag1)
                    effectsManager.SwirlSetup();
                if (flag2)
                {
                    var drawSceneNormalMethod = typeof(Scene).GetMethod("DrawSceneNormal", BindingFlags.NonPublic | BindingFlags.Instance);
                    drawSceneNormalMethod.Invoke(__instance, new object[] { gameTime, spriteBatch });
                }
                else
                {
                    effectsManager.DrawUpdatedSwirlToRT(gameTime, spriteBatch);
                    effectsManager.DrawScreenSwirl(gameTime, spriteBatch);
                }
                if (flag1)
                {
                    effectsManager.DrawUpdatedSwirlToRT(gameTime, spriteBatch);
                    effectsManager.DrawScreenSwirl(gameTime, spriteBatch);
                }

                spriteBatch.Begin(SpriteSortMode.Deferred, null, pointSamplerState, null, null);
                effectsManager.DrawScreenFade(gameTime, spriteBatch);
                spriteBatch.End();
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                var gracefullyRecovered = TryToRepairSpriteBatchState(spriteBatch);
                if (IsRecognizedEnumerationModifiedError(ex) && gracefullyRecovered)
                {
                    _log.LogWarning($"DrawEngine failed in {nameof(DrawScenePatch)}.{nameof(Prefix)} but DLCQuestipelago was able to recover gracefully");
                }
                else
                {
                    _log.LogError($"Failed in {nameof(DrawScenePatch)}.{nameof(Prefix)}:\n\t{ex}");
                    if (gracefullyRecovered)
                    {
                        _log.LogInfo($"DLCQuestipelago was able to recover gracefully from the error");
                    }
                }

                return false; // don't run original logic
            }
        }

        private static bool TryToRepairSpriteBatchState(SpriteBatch spriteBatch)
        {
            Debugger.Break();
            try
            {
                var inBeginEndPairField = typeof(SpriteBatch).GetField("inBeginEndPair",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var inBeginEndPair = (bool)inBeginEndPairField.GetValue(spriteBatch);
                if (inBeginEndPair)
                {
                    spriteBatch.End();
                }

                return true;
            }
            catch (Exception innerEx)
            {
                _log.LogError(
                    $"Failed in {nameof(DrawScenePatch)}.{nameof(Prefix)} inner catch code, trying to reset the spritebatch:\n\t{innerEx}");
                Debugger.Break();
                return false;
            }
        }

        public static bool IsRecognizedEnumerationModifiedError(Exception ex)
        {
            if (ex is not InvalidOperationException invalidOperationException)
            {
                return false;
            }

            const string moveNextRare = "Enumerator.MoveNextRare()";
            const string collectionWasModified = "Collection was modified";
            const string enumerationMayNotExecute = "enumeration operation may not execute";

            if (!invalidOperationException.StackTrace.Contains(moveNextRare) ||
                !invalidOperationException.Message.Contains(collectionWasModified) ||
                !invalidOperationException.Message.Contains(enumerationMayNotExecute))
            {
                return false;
            }

            return true;
        }
    }
}
