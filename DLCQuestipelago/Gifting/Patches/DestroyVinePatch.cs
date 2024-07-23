using System;
using System.Diagnostics;
using DLCLib.World.Props;
using DLCQuestipelago.Extensions;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Gifting.Patches
{
    [HarmonyPatch(typeof(Vine))]
    [HarmonyPatch("OnDestruction")]
    public static class DestroyVinePatch
    {
        private static ILogger _logger;
        private static GiftSender _giftSender;

        public static void Initialize(ILogger logger, GiftSender giftSender)
        {
            _logger = logger;
            _giftSender = giftSender;
        }

        //protected override void OnDestruction()
        public static void Postfix(Vine __instance)
        {
            try
            {
                var physicsObject = __instance.GetPhysicsObject();
                _giftSender.SendVineGift((int)physicsObject.Position.X, (int)physicsObject.Position.Y).FireAndForget();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DestroyBushPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
