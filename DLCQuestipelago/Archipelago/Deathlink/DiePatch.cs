using BepInEx.Logging;
using DLCLib;
using DLCLib.DLC;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.Archipelago.Deathlink
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.Die))]
    public static class DiePatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static bool _isCurrentlyReceivingDeathlink;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
            _isCurrentlyReceivingDeathlink = false;
        }

        public static void ReceiveDeathLink()
        {
            if (!_archipelago.SlotData.DeathLink)
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
            if (!_archipelago.SlotData.DeathLink)
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

            _archipelago.SendDeathLinkAsync(_archipelago.SlotData.SlotName, killerType);

            return true; // run original logic
        }
    }
}
