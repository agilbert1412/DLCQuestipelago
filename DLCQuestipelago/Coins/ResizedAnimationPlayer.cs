using System;
using System.Diagnostics;
using System.Reflection;
using DLCLib.Render;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.Coins
{
    public class ResizedAnimationPlayer: AnimationPlayer
    {
        public ResizedAnimationPlayer(Vector2 scale) : base(scale)
        {
        }

        public Vector2 Scale => this.scale;
    }


    [HarmonyPatch(typeof(AnimationPlayer))]
    [HarmonyPatch(nameof(AnimationPlayer.Update))]
    public static class ResizedAnimationPlayerUpdatePatch
    {
        // public bool Update(float dt)
        public static void Postfix(AnimationPlayer __instance, float dt, ref bool __result)
        {
            try
            {
                if (__instance is not ResizedAnimationPlayer resizedAnimationPlayer)
                {
                    return;
                }

                // private float animationHeightInMeters;
                var animationHeightInMetersField = typeof(AnimationPlayer).GetField("animationHeightInMeters", BindingFlags.NonPublic | BindingFlags.Instance);

                // private Vector2 origin;
                var originField = typeof(AnimationPlayer).GetField("origin", BindingFlags.NonPublic | BindingFlags.Instance);
                var origin = (Vector2)originField.GetValue(resizedAnimationPlayer);

                animationHeightInMetersField.SetValue(resizedAnimationPlayer, origin.Y * resizedAnimationPlayer.Scale.Y);
                return;
            }
            catch (Exception ex)
            {
                Debugger.Break();
                return;
            }
        }
    }
}
