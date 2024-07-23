using System;
using System.Collections.Generic;
using System.Reflection;
using DLCLib.Physics;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.AntiCrashes
{
    [HarmonyPatch(typeof(PhysicsManager))]
    [HarmonyPatch("PerformStep")]
    public static class PhysicsManagerPerformStepPatch
    {
        private static readonly Vector2 MAX_VELOCITY = new Vector2(30f);

        private static ILogger _logger;
        private static NodeCleaner _nodeCleaner;

        public static void Initialize(ILogger logger, NodeCleaner nodeCleaner)
        {
            _logger = logger;
            _nodeCleaner = nodeCleaner;
        }

        // protected void PerformStep(float dt)
        private static bool Prefix(PhysicsManager __instance, float dt)
        {
            try
            {
                var dynamicPhysicalEntitiesField = typeof(PhysicsManager).GetField("dynamicPhysicalEntities",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var dynamicPhysicalEntity in
                         (List<IPhysical>)dynamicPhysicalEntitiesField.GetValue(__instance))
                {
                    var physicsObject1 = dynamicPhysicalEntity.GetPhysicsObject();
                    physicsObject1.Velocity += physicsObject1.Acceleration * dt;
                    if (!physicsObject1.IgnoreGravity)
                    {
                        physicsObject1.Velocity += (physicsObject1.UseSlowGravity
                            ? PhysicsManager.SLOW_GRAVITY
                            : PhysicsManager.GRAVITY) * dt;
                    }

                    if (physicsObject1.IsOnGround)
                    {
                        physicsObject1.Velocity.X *= (float)(1.0 - (double)physicsObject1.GroundFriction * (double)dt);
                    }
                    else
                    {
                        physicsObject1.Velocity.X *= (float)(1.0 - (double)physicsObject1.AirFriction * (double)dt);
                    }

                    physicsObject1.Velocity = Vector2.Clamp(physicsObject1.Velocity, -MAX_VELOCITY, MAX_VELOCITY);
                    var physicsObject2 = physicsObject1;
                    physicsObject2.Position += physicsObject1.Velocity * dt;
                    physicsObject1.Acceleration = Vector2.Zero;
                    physicsObject1.IsOnGround = false;
                    physicsObject1.IsAtCeiling = false;
                    physicsObject1.IsOnWall = false;
                }

                var doCollisionMethod =
                    typeof(PhysicsManager).GetMethod("DoCollision", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var dynamicPhysicalEntity in
                         (List<IPhysical>)dynamicPhysicalEntitiesField.GetValue(__instance))
                {
                    var physicsObject = dynamicPhysicalEntity.GetPhysicsObject();
                    if (!physicsObject.IsActive) continue;
                    doCollisionMethod.Invoke(__instance, new[] { physicsObject });
                    physicsObject.OffGroundTime += dt;
                    physicsObject.OffWallTime += dt;
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PhysicsManagerPerformStepPatch)}.{nameof(Prefix)}:\n\t{ex}");
                // Debugger.Break();

                if (ex.Message.Equals(NodeCleaner.NODE_DOES_NOT_HAVE_PARENT_ERROR, StringComparison.InvariantCultureIgnoreCase))
                {
                    _nodeCleaner.CleanItemsToNodesRelationships(__instance);
                }

                return false; // don't run original logic
            }
        }
    }
}
