using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using Core;
using Core.Spatial;
using DLCLib;
using DLCLib.Effects;
using DLCLib.Input;
using DLCLib.NIS;
using DLCLib.Physics;
using DLCLib.World;
using HarmonyLib;
using HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DLCQuestipelago.AntiCrashes
{
    [HarmonyPatch(typeof(Scene))]
    [HarmonyPatch(nameof(Scene.Update))]
    public static class UpdateScenePatch
    {
        private static ManualLogSource _log;
        private static NodeCleaner _nodeCleaner;

        public static void Initialize(ManualLogSource log, NodeCleaner nodeCleaner)
        {
            _log = log;
            _nodeCleaner = nodeCleaner;
        }

        // public void Update(GameTime gameTime)
        private static bool Prefix(Scene __instance, GameTime gameTime)
        {
            try
            {
                var dt = (float)gameTime.ElapsedGameTime.TotalSeconds * __instance.TimeScale;
                var timers = (TimerCollection)TimersField.GetValue(__instance);
                timers.Update(dt);
                var interpolators = (InterpolatorCollection)InterpolatorsField.GetValue(__instance);
                interpolators.Update(dt);
                if (__instance.IsLoading)
                {
                    return false; // don't run original logic
                }

                var nisManager = (NISManager)NisManagerField.GetValue(__instance);
                nisManager.Update(dt);
                var effectsManager = (EffectsManager)EffectsManagerField.GetValue(__instance);
                effectsManager.Update(dt);
                UpdateAddedEntitiesMethod.Invoke(__instance, new object[] {});
                if (nisManager.HasControl)
                {
                    var thePlayer = (Player)ThePlayerField.GetValue(__instance);
                    thePlayer.PlayerInput = PlayerInput.Empty;
                }

                var tempQueryList = (List<QuadTreeItem<Entity>>)TempQueryListField.GetValue(__instance);
                tempQueryList.Clear();
                var physicsManager = (PhysicsManager)PhysicsManagerField.GetValue(__instance);
                physicsManager.QueryAABB(__instance.Camera.LargerBoundsInMeters, ref tempQueryList);
                tempQueryList = (List<QuadTreeItem<Entity>>)TempQueryListField.GetValue(__instance);
                foreach (var tempQuery in tempQueryList)
                {
                    tempQuery.Entity.PrePhysicsUpdate(dt);
                }

                physicsManager.Step(dt);
                tempQueryList = (List<QuadTreeItem<Entity>>)TempQueryListField.GetValue(__instance);
                foreach (var tempQuery in tempQueryList)
                {
                    tempQuery.Entity.PostPhysicsUpdate(dt);
                }

                UpdateRemovedEntitiesMethod.Invoke(__instance, new object[] { });
                var map = (Map)MapField.GetValue(__instance);
                map.Update(dt);
                var currentCamera = (Camera)CurrentCameraField.GetValue(__instance);
                currentCamera.Update(dt);

                var hudManager = (HUDManager)HudManagerField.GetValue(__instance);
                hudManager.Update(gameTime);

                var effect = (BasicEffect)EffectField.GetValue(__instance);
                effect.View = currentCamera.ViewMatrix;
                if (Scene.useBasicEffect)
                {
                    return false; // don't run original logic
                }

                var scale = 90f;
                var vector2 = new Vector2(1280f, 720f) * (1f / scale) * 0.5f;
                var transformMatrixForNonBasicEffectNewValue = effect.World *
                                                               Matrix.CreateTranslation(vector2.X - 0.5f,
                                                                   vector2.Y - 0.5f, 0.0f) * effect.View *
                                                               Matrix.CreateScale(scale);
                TransformMatrixForNonBasicEffectField.SetValue(__instance, transformMatrixForNonBasicEffectNewValue);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {

                var gracefullyRecovered = TryToRepairItemsNodesState(__instance, ex);
                if (DrawScenePatch.IsRecognizedEnumerationModifiedError(ex) && gracefullyRecovered)
                {
                    _log.LogWarning($"PhysicsEngine failed in {nameof(UpdateScenePatch)}.{nameof(Prefix)} but DLCQuestipelago was able to recover gracefully");
                }
                else
                {
                    _log.LogError($"Failed in {nameof(UpdateScenePatch)}.{nameof(Prefix)}:\n\t{ex}");
                    if (gracefullyRecovered)
                    {
                        _log.LogInfo($"DLCQuestipelago was able to recover gracefully from the error");
                    }
                }

                return false; // don't run original logic
            }
        }

        private static bool TryToRepairItemsNodesState(Scene scene, Exception ex)
        {
            // Debugger.Break();

            if (ex.Message.Equals(NodeCleaner.NODE_DOES_NOT_HAVE_PARENT_ERROR, StringComparison.InvariantCultureIgnoreCase))
            {
                var physicsManager = (PhysicsManager)PhysicsManagerField.GetValue(scene);
                _nodeCleaner.CleanItemsToNodesRelationships(physicsManager);
            }

            return true;
        }


        private static readonly FieldInfo TimersField = typeof(Scene).GetField("timers", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo InterpolatorsField = typeof(Scene).GetField("interpolators", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo NisManagerField = typeof(Scene).GetField("nisManager", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo EffectsManagerField = typeof(Scene).GetField("effectsManager", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo ThePlayerField = typeof(Scene).GetField("thePlayer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo TempQueryListField = typeof(Scene).GetField("tempQueryList", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo PhysicsManagerField = typeof(Scene).GetField("physicsManager", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo MapField = typeof(Scene).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo CurrentCameraField = typeof(Scene).GetField("currentCamera", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo HudManagerField = typeof(Scene).GetField("hudManager", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo EffectField = typeof(Scene).GetField("effect", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo TransformMatrixForNonBasicEffectField = typeof(Scene).GetField("transformMatrixForNonBasicEffect", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo UpdateAddedEntitiesMethod = typeof(Scene).GetMethod("UpdateAddedEntities", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo UpdateRemovedEntitiesMethod = typeof(Scene).GetMethod("UpdateRemovedEntities", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}
