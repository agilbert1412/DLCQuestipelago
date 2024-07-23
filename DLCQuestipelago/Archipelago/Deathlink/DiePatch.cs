using System;
using System.Diagnostics;
using DLCLib;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Archipelago.Deathlink
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.Die))]
    public static class DiePatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;
        private static bool _isCurrentlyReceivingDeathlink;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _isCurrentlyReceivingDeathlink = false;
        }

        public static void ReceiveDeathLink()
        {
            if (_archipelago?.SlotData == null || _archipelago.SlotData.DeathLink == false)
            {
                return;
            }

            _isCurrentlyReceivingDeathlink = true;
            SceneManager.Instance.CurrentScene.Player.Die(null);
            _isCurrentlyReceivingDeathlink = false;
        }

        // public void Die(Entity killer)
        private static bool Prefix(Player __instance, Entity killer)
        {
            try
            {
                if (_archipelago.SlotData.DeathLink == false)
                {
                    return true;
                }

                if (!SceneManager.Instance.CurrentScene.NISManager.HasControl && !__instance.IsAlive)
                {
                    return true; // run original logic
                }

                if (_isCurrentlyReceivingDeathlink)
                {
                    return true; // run original logic
                }

                var killerString = killer.ToString();
                var killerType = killer.GetType().Name;

                _archipelago.SendDeathLinkAsync(killerType);

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DiePatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
