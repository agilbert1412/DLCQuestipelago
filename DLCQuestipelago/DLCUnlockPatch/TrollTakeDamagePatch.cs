using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using DLCLib.Character;
using DLCLib.DLC;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TrollNPC))]
    [HarmonyPatch(nameof(TrollNPC.TakeDamage))]
    public static class TrollTakeDamagePatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public override void TakeDamage(int damageAmount, Vector2 direction)
        private static void Postfix(TrollNPC __instance, int damageAmount, Vector2 direction)
        {
            try
            {
                DLCManager.Instance.UnlockPack("gun");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TrollTakeDamagePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
