using Core;
using Core.Input;
using DLCLib;
using DLCLib.Conversation;
using DLCLib.Physics;
using DLCLib.Render;
using DLCLib.Screens;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.ItemShufflePatches;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace DLCQuestipelago.QualityOfLife
{
    [HarmonyPatch(typeof(DLCGameplayScreen))]
    [HarmonyPatch(nameof(DLCGameplayScreen.HandleInput))]
    public static class InputPatch
    {
        public const string DEFAULT_TELEPORT_SPAWN_KEY = "Q";

        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public override void HandleInput(InputState input)
        private static bool Prefix(DLCGameplayScreen __instance, InputState input)
        {
            try
            {
                HandleTeleportInput(__instance, input);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(InputPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

        }

        private static void HandleTeleportInput(DLCGameplayScreen screen, InputState input)
        {
            var currentKeyboardState = input.CurrentKeyboardState;
            var pressedKeys = currentKeyboardState.GetPressedKeys();
            var teleportKeysString = Plugin.Instance.APConnectionInfo.TeleportToSpawnKey;
            if (string.IsNullOrWhiteSpace(teleportKeysString))
            {
                return;
            }

            var teleportKeys = new List<Keys>();
            foreach (var teleportKeyChar in teleportKeysString)
            {
                var teleportKeyString = teleportKeyChar.ToString();
                if (string.IsNullOrWhiteSpace(teleportKeyString))
                {
                    continue;
                }
                if (Enum.TryParse<Keys>(teleportKeyChar.ToString(), true, out var key))
                {
                    teleportKeys.Add(key);
                }
            }

            if (pressedKeys.Any(x => teleportKeys.Contains(x)))
            {
                TeleportToSpawn(screen);
            }

            return;
        }

        private static void TeleportToSpawn(DLCGameplayScreen screen)
        {
            var player = SceneManager.Instance?.CurrentScene?.Player;
            if (player == null)
            {
                return;
            }

            var playerStateField = typeof(Player).GetField("playerState", BindingFlags.NonPublic | BindingFlags.Instance);
            var animPlayerField = typeof(Player).GetField("animPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            var physicsObjectField = typeof(Player).GetField("physicsObject", BindingFlags.NonPublic | BindingFlags.Instance);
            var maxHealthField = typeof(Player).GetField("maxHealth", BindingFlags.NonPublic | BindingFlags.Instance);

            var playerState = (PlayerStateEnum)(int)playerStateField.GetValue(player);
            var animPlayer = (AnimationPlayer)animPlayerField.GetValue(player);
            var physicsObject = (PhysicsObject)physicsObjectField.GetValue(player);
            var maxHealth = (int)maxHealthField.GetValue(player);

            Singleton<SceneManager>.Instance.CurrentScene.LocatorManager.GetPosition("spawn", out var respawnPos);

            animPlayer.Rotation = 0.0f;
            player.Teleport(respawnPos);
            playerStateField.SetValue(player, PlayerStateEnum.Idle);
            Singleton<SceneManager>.Instance.CurrentScene.EffectsManager.AddTeleportSmoke(physicsObject.AABB.BottomCenter, Vector2.Zero, Units.MetersPerPixel);
            player.Health = maxHealth;
        }

        private enum PlayerStateEnum
        {
            Idle = 0,
            Running = 1,
            Jumping = 2,
            Dying = 3,
            Dead = 4,
        }
    }
}
