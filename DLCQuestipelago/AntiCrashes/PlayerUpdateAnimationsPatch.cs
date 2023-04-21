using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.Audio;
using DLCLib.DLC;
using DLCLib.Physics;
using DLCLib.Render;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DLCQuestipelago.AntiCrashes
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("UpdateAnimation")]
    public static class PlayerUpdateAnimationsPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // protected void UpdateAnimation(float dt)
        private static bool Prefix(Player __instance, float dt)
        {
            try
            {
                OriginalUpdateAnimations(__instance, dt);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(PlayerUpdateAnimationsPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return false; // don't run original logic
            }
        }

        private static void OriginalUpdateAnimations(Player instance, float dt)
        {
            var playerStateField = typeof(Player).GetField("playerState", BindingFlags.NonPublic | BindingFlags.Instance);
            var animPlayerField = typeof(Player).GetField("animPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            var physicsObjectField = typeof(Player).GetField("physicsObject", BindingFlags.NonPublic | BindingFlags.Instance);
            var stepCountField = typeof(Player).GetField("stepCount", BindingFlags.NonPublic | BindingFlags.Instance);

            var playerState = (PlayerStateEnum)(int)playerStateField.GetValue(instance);
            var animPlayer = (AnimationPlayer)animPlayerField.GetValue(instance);
            var physicsObject = (PhysicsObject)physicsObjectField.GetValue(instance);

            PlayCorrectAnimation(instance, playerState, animPlayer, physicsObject);

            animPlayer.SpriteEffects = instance.FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (!animPlayer.Update(dt) || playerState != PlayerStateEnum.Running)
            {
                return;
            }

            var stepCount = (int)stepCountField.GetValue(instance);
            stepCountField.SetValue(instance, stepCount + 1);
            Singleton<SceneManager>.Instance.CurrentScene.EffectsManager.AddWalkPuff(physicsObject.AABB.BottomCenter, new Vector2(MathHelper.Clamp(-physicsObject.Velocity.X, -0.5f, 0.5f), 0.0f), Units.MetersPerPixel);
            if ((int)stepCountField.GetValue(instance) <= 1)
            {
                return;
            }

            DLCAudioManager.Instance.PlaySound("footstep");
            stepCountField.SetValue(instance, 0);
        }

        private static void PlayCorrectAnimation(Player instance, PlayerStateEnum playerState, AnimationPlayer animPlayer, PhysicsObject physicsObject)
        {
            var overrideAnimationField = typeof(Player).GetField("overrideAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
            var overrideAnimation = (bool)overrideAnimationField.GetValue(instance);

            if (overrideAnimation)
            {
                return;
            }

            var attackStateField = typeof(Player).GetField("attackState", BindingFlags.NonPublic | BindingFlags.Instance);
            var animationsField = typeof(Player).GetField("animations", BindingFlags.NonPublic | BindingFlags.Instance);
            var attackState = (AttackStateEnum)(int)attackStateField.GetValue(instance);
            var animations = (Dictionary<string, Animation>)animationsField.GetValue(instance);

            if (playerState == PlayerStateEnum.Dead)
            {
                PlayAnimation(animPlayer, animations, "dead");
            }
            else if (Singleton<DLCManager>.Instance.IsPurchased("animation", true))
            {
                if (attackState == AttackStateEnum.SwingingSword || attackState == AttackStateEnum.FiringGun)
                {
                    PlayAnimation(animPlayer, animations, "attack");
                }
                else
                {
                    switch (playerState)
                    {
                        case PlayerStateEnum.Idle:
                            PlayAnimation(animPlayer, animations, "idle");
                            break;
                        case PlayerStateEnum.Running:
                            animations["run"].FrameTime =
                                MathHelper.Clamp(MathHelper.Lerp(0.5f, 0.1f, Math.Abs(physicsObject.Velocity.X) / 5f), 0.1f,
                                    0.5f);
                            PlayAnimation(animPlayer, animations, "run");
                            break;
                        case PlayerStateEnum.Jumping:
                            PlayAnimation(animPlayer, animations, "jump");
                            break;
                    }
                }
            }

            if (!Singleton<DLCManager>.Instance.IsPurchased("animation", true) && instance.Inventory.HasSword)
            {
                PlayAnimation(animPlayer, animations, "idle");
            }
        }

        private static void PlayAnimation(AnimationPlayer animationPlayer, Dictionary<string, Animation> allAnimations,
            string chosenAnimation)
        {
            if (allAnimations.ContainsKey(chosenAnimation))
            {
                animationPlayer.Play(allAnimations[chosenAnimation]);
            }
            else
            {
                animationPlayer.Play(allAnimations.Values.First());
            }
        }

        private enum PlayerStateEnum
        {
            Idle = 0,
            Running = 1,
            Jumping = 2,
            Dying = 3,
            Dead = 4,
        }

        private enum AttackStateEnum
        {
            Idle = 0,
            SwingingSword = 1,
            FiringGun = 2,
            Cooldown = 3,
        }
    }
}
