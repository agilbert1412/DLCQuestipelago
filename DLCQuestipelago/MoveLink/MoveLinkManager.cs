using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Archipelago.MultiClient.Net.Packets;
using DLCLib;
using DLCLib.Physics;
using DLCLib.Screens;
using GameStateManagement;
using KaitoKid.ArchipelagoUtilities.Net.BouncePackets;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace DLCQuestipelago.MoveLink
{
    public static class MoveLinkManager
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static List<SharedMovement> _queuedMovements;

        // protected List<IPhysical> dynamicPhysicalEntities;
        private static FieldInfo _entitiesField = typeof(PhysicsManager).GetField("dynamicPhysicalEntities", BindingFlags.Instance | BindingFlags.NonPublic);

        // private void DoCollision(PhysicsObject objA)
        private static MethodInfo _doCollisionMethod = typeof(PhysicsManager).GetMethod("DoCollision", BindingFlags.Instance | BindingFlags.NonPublic);

        private const float TILE_FACTOR = 2;
        private const float EPSILON = 0.1f;

        private static SharedMovement _accumulatedMoveLink;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _queuedMovements = new List<SharedMovement>();
            _accumulatedMoveLink = new SharedMovement(0, 0, 0);
        }

        public static void AddMoveLinkMovement(float time, float x, float y)
        {
            if (!FoolManager.ShouldPrank() || time <= 0 || (Math.Abs(x) < EPSILON && Math.Abs(y) < EPSILON))
            {
                return;
            }

            _queuedMovements.Add(new SharedMovement(time, x, y));
        }

        private static int _frameCount = 0;

        public static void UpdateMove(GameTime gameTime)
        {
            const uint moveLinkSendInterval = 30;
            const uint moveLinkApplyInterval = 1;

            _frameCount += 1;
            if (!FoolManager.ShouldPrank())
            {
                return;
            }

            AccumulateMoveLink(gameTime);

            if (_frameCount % moveLinkSendInterval == 0)
            {
                SendMoveLink(moveLinkSendInterval);
            }
            if (_frameCount % moveLinkApplyInterval == 0)
            {
                ApplyMoveLink((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        private static void AccumulateMoveLink(GameTime gameTime)
        {
            var player = SceneManager.Instance.CurrentScene.Player;
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _accumulatedMoveLink.Time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _accumulatedMoveLink.X += player.GetPhysicsObject().Velocity.X * time;
            _accumulatedMoveLink.Y += player.GetPhysicsObject().Velocity.Y * time;
        }

        public static void SendMoveLink(uint elapsedFrames)
        {
            try
            {
                if (_logger == null || _archipelago == null || !FoolManager.ShouldPrank())
                {
                    return;
                }

                var session = _archipelago.GetSession();
                var slot = $"{session.ConnectionInfo.Slot}-{Plugin.Instance.UniqueIdentifier}";

                var x = _accumulatedMoveLink.X / TILE_FACTOR;
                var y = _accumulatedMoveLink.Y / TILE_FACTOR;

                if (Math.Abs(x) < EPSILON && Math.Abs(y) < EPSILON)
                {
                    return;
                }

                _archipelago.SendMoveLinkPacket(slot, _accumulatedMoveLink.Time, x, y);

                //_logger.LogInfo($"Sent {ArchipelagoClient.MOVE_LINK_TAG} packet{Environment.NewLine}" +
                //                $"  slot: {slot}{Environment.NewLine}" +
                //                $"  timespan: {_accumulatedMoveLink.Time}{Environment.NewLine}" +
                //                $"  x: {x}{Environment.NewLine}" +
                //                $"  y: {y}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SendMoveLink)}:\n{ex}");
                return;
            }
            finally
            {
                _accumulatedMoveLink.Time = 0;
                _accumulatedMoveLink.X = 0;
                _accumulatedMoveLink.Y = 0;
            }
        }

        public static void ApplyMoveLink(float elapsedSeconds)
        {
            var scene = SceneManager.Instance.CurrentScene;
            if (!FoolManager.ShouldPrank() || !_queuedMovements.Any() || ScreenManager.Instance.GetScreens().Any(x => x is MainMenuScreen))
            {
                return;
            }

            var player = scene.Player;

            var currentMove = _queuedMovements[0];

            var x = currentMove.X;
            var y = currentMove.Y;

            while (currentMove.Time <= 0 || (Math.Abs(currentMove.X) < EPSILON && Math.Abs(currentMove.Y) < EPSILON))
            {
                _queuedMovements.RemoveAt(0);
                if (!_queuedMovements.Any())
                {
                    return;
                }

                currentMove = _queuedMovements[0];
                x = currentMove.X;
                y = currentMove.Y;
            }
            if (currentMove.Time <= elapsedSeconds)
            {
                _queuedMovements.RemoveAt(0);
            }
            else if (currentMove.Time > elapsedSeconds)
            {
                var factor = elapsedSeconds / currentMove.Time;
                var factoredX = x * factor;
                var factoredY = y * factor;
                x = factoredX;
                y = factoredY;
                currentMove.X -= factoredX;
                currentMove.Y -= factoredY;
                currentMove.Time -= elapsedSeconds;
            }

            MovePlayer(scene, player, x * TILE_FACTOR, y * TILE_FACTOR);
        }

        private static void MovePlayer(Scene scene, Player player, float x, float y)
        {
            var playerPhysicsObject = player.GetPhysicsObject();
            var currentPosition = playerPhysicsObject.Position;
            playerPhysicsObject.Position = new Vector2(currentPosition.X + x, currentPosition.Y + y);

            var physicsManager = scene.PhysicsManager;

            var entities = (List<IPhysical>)_entitiesField.GetValue(physicsManager);

            foreach (var dynamicPhysicalEntity in entities)
            {
                var physicsObject = dynamicPhysicalEntity.GetPhysicsObject();
                if (physicsObject.IsActive)
                {
                    _doCollisionMethod.Invoke(physicsManager, new []{physicsObject});
                }
            }
        }

        public static void HandleBouncePacket(BouncePacket bouncePacket)
        {
            if (!FoolManager.ShouldPrank())
            {
                return;
            }

            if (!bouncePacket.Tags.Contains(ArchipelagoClient.MOVE_LINK_TAG))
            {
                return;
            }

            var slot = bouncePacket.GetDataValue("slot", "");
            if (string.IsNullOrWhiteSpace(slot) || slot.Contains(Plugin.Instance.UniqueIdentifier))
            {
                return;
            }
            var timespan = bouncePacket.GetDataValue("timespan", 0.25f);
            var x = bouncePacket.GetDataValue("x", 0f);
            var y = bouncePacket.GetDataValue("y", 0f);

            AddMoveLinkMovement(timespan, x, y);

            //_logger.LogInfo($"Received {ArchipelagoClient.MOVE_LINK_TAG} packet{Environment.NewLine}" +
            //               $"  slot: {slot}{Environment.NewLine}" +
            //               $"  timespan: {timespan}{Environment.NewLine}" +
            //               $"  x: {x}{Environment.NewLine}" +
            //               $"  y: {y}");
        }
    }
}
