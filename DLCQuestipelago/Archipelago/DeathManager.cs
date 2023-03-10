using BepInEx.Logging;
using HarmonyLib;

namespace DLCQuestipelago.Archipelago
{
    public class DeathManager
    {
        private static ManualLogSource _monitor;
        private static ArchipelagoClient _archipelago;
        private Harmony _harmony;

        private static bool _isCurrentlyReceivingDeathLink = false;

        public DeathManager(ManualLogSource monitor, Harmony harmony, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _harmony = harmony;
            _archipelago = archipelago;
        }

        public static void ReceiveDeathLink()
        {
            // TODO
        }

        private static void SendDeathLink(string cause)
        {
            if (_isCurrentlyReceivingDeathLink)
            {
                _isCurrentlyReceivingDeathLink = false;
                return;
            }

            _archipelago.SendDeathLink(_archipelago.SlotData.SlotName, cause);
        }

        public void HookIntoDeathlinkEvents()
        {
            if (!_archipelago.SlotData.DeathLink)
            {
                return;
            }

            HookIntoDeathEvent();
        }

        private void HookIntoDeathEvent()
        {
            // TODO
        }
    }
}
