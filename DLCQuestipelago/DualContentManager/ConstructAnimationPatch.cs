using System.Collections.Generic;
using System.Linq;
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
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        //public static void ConstructAnimation(AnimationData animData, out Animation anim)
        private static bool Prefix(AnimationData animData, out Animation anim)
        {
            var spriteSheetsByName = DLCQuestipelagoMod.DualAssetManager.GetSpriteSheetsByName(animData.SpriteSheetName);
            var correctSpriteSheet = spriteSheetsByName.First(x => x.Contains(animData.FrameNames.First()));
            var sourceRects = new List<Rectangle>();
            foreach (var frameName in animData.FrameNames)
            {
                sourceRects.Add(correctSpriteSheet.SourceRectangle(frameName));
            }

            anim = new Animation(correctSpriteSheet.Texture, sourceRects, animData.FrameTime);

            return false; // don't run original logic
        }
    }
}
