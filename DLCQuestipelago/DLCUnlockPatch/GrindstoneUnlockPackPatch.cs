﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Core;
using DLCLib;
using DLCLib.Audio;
using DLCLib.DLC;
using DLCLib.Physics;
using DLCLib.Render;
using DLCLib.World.Props;
using KaitoKid.ArchipelagoUtilities.Net;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Timer = Core.Timer;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(Grindstone))]
    [HarmonyPatch(nameof(Grindstone.Activate))]
    public static class GrindstoneUnlockPackPatch
    {
        private const int MAX_GRINDS = 10000;
        private const long JOULES_PER_GRIND = 150;
        private const string TIME_IS_MONEY = "Time is Money Pack";
        private const string DAY_ONE_PATCH = "Day One Patch Pack";
        private const string BANKING_TEAM_KEY = "EnergyLink{0}";

        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static int _grindCount;
        private static string _energyLinkKey;

        private static PropertyInfo _isCompleteProperty;
        private static FieldInfo _grindCountField;
        private static FieldInfo _idleTimerField;
        private static FieldInfo _animPlayerField;
        private static FieldInfo _physicsObjectField;
        private static FieldInfo _idleAnimField;
        private static FieldInfo _spinAnimField;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            GetFieldAndPropertyInfos();
            _grindCount = 0;
            _energyLinkKey = string.Format(BANKING_TEAM_KEY, _archipelago.GetTeam());
        }

        private static void GetFieldAndPropertyInfos()
        {
            _isCompleteProperty = typeof(Grindstone).GetProperty("IsComplete",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            // protected int grindCount;
            _grindCountField = typeof(Grindstone).GetField("grindCount", BindingFlags.NonPublic | BindingFlags.Instance);
            // protected Timer idleTimer;
            _idleTimerField = typeof(Grindstone).GetField("idleTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            // protected AnimationPlayer animPlayer;
            _animPlayerField = typeof(Grindstone).GetField("animPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            // protected PhysicsObject physicsObject;
            _physicsObjectField = typeof(Grindstone).GetField("physicsObject", BindingFlags.NonPublic | BindingFlags.Instance);
            // protected Animation idleAnim;
            _idleAnimField = typeof(Grindstone).GetField("idleAnim", BindingFlags.NonPublic | BindingFlags.Instance);
            // protected Animation spinAnim;
            _spinAnimField = typeof(Grindstone).GetField("spinAnim", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        // public bool Activate()
        public static bool Prefix(Grindstone __instance, ref bool __result)
        {
            try
            {
                __instance.Enable();
                if (_locationChecker.IsLocationMissing("Sword"))
                {
                    _isCompleteProperty.SetValue(__instance, false);
                }

                var session = _archipelago.GetSession();
                var isComplete = (bool)_isCompleteProperty.GetValue(__instance);
                if (isComplete)
                {
                    GrindOnceEvenIfCompleted(session, __instance);
                    return false;
                }

                TryToFinishGrindingUsingEnergyLink(session, __instance);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GrindstoneUnlockPackPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }

        public static void Postfix(Grindstone __instance, ref bool __result)
        {
            try
            {
                _grindCount++;
                if (_grindCount >= 9)
                {
                    DLCManager.Instance.UnlockPack("grindstone");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GrindstoneUnlockPackPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }

        private static void GrindOnceEvenIfCompleted(ArchipelagoSession session, Grindstone grindstone)
        {
            var hasTimeIsMoney = _archipelago.HasReceivedItem(TIME_IS_MONEY, out _);
            var hasDayOnePatch = _archipelago.HasReceivedItem(DAY_ONE_PATCH, out _);
            if (hasDayOnePatch)
            {
                ThreadPool.QueueUserWorkItem((o) => SendOneGrindToEnergyLink(session, hasTimeIsMoney));
            }
            else
            {
                SendOneGrindToEnergyLink(session, hasTimeIsMoney);
            }

            PlayGrindAnimation(grindstone, hasTimeIsMoney);
        }

        private static void SendOneGrindToEnergyLink(ArchipelagoSession session, bool hasTimeIsMoney)
        {
            if (session == null)
            {
                return;
            }

            var joulesPerGrind = JOULES_PER_GRIND;

            if (hasTimeIsMoney)
            {
                joulesPerGrind *= 1000;
            }

            _logger.LogInfo($"Already finished grinding, sending {joulesPerGrind} joules into the EnergyLink");
            session.DataStorage[Scope.Global, _energyLinkKey] += joulesPerGrind;
        }

        private static void PlayGrindAnimation(Grindstone grindstone, bool hasTimeIsMoney)
        {
            DLCAudioManager.Instance.PlaySound("grindstone");
            var idleTimer = (Timer)_idleTimerField.GetValue(grindstone);
            var spinAnim = (Animation)_spinAnimField.GetValue(grindstone);
            var idleAnim = (Animation)_idleAnimField.GetValue(grindstone);
            var animPlayer = (AnimationPlayer)_animPlayerField.GetValue(grindstone);
            animPlayer.Play(spinAnim);
            if (idleTimer != null)
            {
                idleTimer.Stop();
                _idleTimerField.SetValue(grindstone, null);
            }

            var newIdleTimer = Singleton<SceneManager>.Instance.CurrentScene.Timers.Create(1f, false, (Action<Timer>)(stopStone =>
            {
                animPlayer.Play(idleAnim);
                _idleTimerField.SetValue(grindstone, null);
            }));
            _idleTimerField.SetValue(grindstone, newIdleTimer);
            var sparks = hasTimeIsMoney ? 4 : 1;
            var physicsObject = (PhysicsObject)_physicsObjectField.GetValue(grindstone);
            for (var i = 0; i < sparks; ++i)
            {
                Singleton<SceneManager>.Instance.CurrentScene.EffectsManager.AddSparks(physicsObject.AABB.TopCenter, Vector2.Zero, Units.MetersPerPixel);
            }
        }

        private static void TryToFinishGrindingUsingEnergyLink(ArchipelagoSession session, Grindstone grindstone)
        {
            var grindsRemaining = MAX_GRINDS - (int)_grindCountField.GetValue(grindstone);
            var joulesNeeded = grindsRemaining * JOULES_PER_GRIND * 100;

            if (joulesNeeded <= 0 || session == null)
            {
                return;
            }

            var hasTimeIsMoney = _archipelago.HasReceivedItem(TIME_IS_MONEY, out _);
            if (!hasTimeIsMoney)
            {
                joulesNeeded *= 1000;
            }
            
            GetEnergyLinkJoulesAmount(session, (amount) => FinishGrindingWithEnergyLink(session, grindstone, amount.Result, joulesNeeded));
        }

        private static void FinishGrindingWithEnergyLink(ArchipelagoSession session, Grindstone grindstone, BigInteger? currentAmountJoules, long joulesNeeded)
        {
            if (currentAmountJoules == null)
            {
                currentAmountJoules = 0;
            }

            if (currentAmountJoules < joulesNeeded)
            {
                _logger.LogInfo($"Not enough energy stored to finish grinding (Current: {currentAmountJoules}, Needed: {joulesNeeded})");
                return;
            }

            session.DataStorage[Scope.Global, _energyLinkKey] -= joulesNeeded;
            _logger.LogInfo($"Used up {joulesNeeded} from the EnergyLink to finish the grindstone!");
            _grindCountField.SetValue(grindstone, 10000);
        }

        private static void GetEnergyLinkJoulesAmount(ArchipelagoSession session, Action<Task<BigInteger?>> callback)
        {
            DataStorageElement value = null;
            try
            {
                value = session.DataStorage[Scope.Global, _energyLinkKey];
                value.GetAsync<BigInteger?>().ContinueWith(callback);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Reading BigInteger from DataStorage key [{_energyLinkKey}]. Value: {value ?? "unknown"}. Message: {ex.Message}");
            }
        }
    }
}
