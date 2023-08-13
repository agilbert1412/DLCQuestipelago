using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.World.Props;
using DLCQuestipelago.Extensions;
using HarmonyLib;

namespace DLCQuestipelago.Gifting.Patches
{
    [HarmonyPatch(typeof(Bush))]
    [HarmonyPatch("OnDestruction")]
    public static class DestroyBushPatch
    {
        private static ManualLogSource _log;
        private static GiftSender _giftSender;

        public static void Initialize(ManualLogSource log, GiftSender giftSender)
        {
            _log = log;
            _giftSender = giftSender;
        }

        //protected override void OnDestruction()
        public static void Postfix(Bush __instance)
        {
            try
            {
                var physicsObject = __instance.GetPhysicsObject();
                _giftSender.SendTreeGift((int)physicsObject.Position.X, (int)physicsObject.Position.Y).FireAndForget();
                return;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(DestroyBushPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
