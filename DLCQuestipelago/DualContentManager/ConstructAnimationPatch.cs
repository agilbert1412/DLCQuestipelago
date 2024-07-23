using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCDataTypes;
using DLCLib.Render;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.DualContentManager
{
    [HarmonyPatch(typeof(AnimationUtil))]
    [HarmonyPatch(nameof(AnimationUtil.ConstructAnimation))]
    public static class ConstructAnimationPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        //public static void ConstructAnimation(AnimationData animData, out Animation anim)
        private static bool Prefix(AnimationData animData, out Animation anim)
        {
            try
            {
                var spriteSheetsByName = Plugin.DualAssetManager.GetSpriteSheetsByName(animData.SpriteSheetName);
                var correctSpriteSheet = spriteSheetsByName.First(x => x.Contains(animData.FrameNames.First()));
                var sourceRects = new List<Rectangle>();
                foreach (var frameName in animData.FrameNames)
                {
                    sourceRects.Add(correctSpriteSheet.SourceRectangle(frameName));
                }

                anim = new Animation(correctSpriteSheet.Texture, sourceRects, animData.FrameTime);

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ConstructAnimationPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                anim = null;
                return true; // run original logic
            }
        }
    }
}
