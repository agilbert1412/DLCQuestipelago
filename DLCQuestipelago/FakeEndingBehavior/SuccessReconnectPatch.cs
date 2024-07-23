using System;
using System.Diagnostics;
using DLCLib.Scripts.LFOD;
using GameStateManagement;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.FakeEndingBehavior
{
    [HarmonyPatch(typeof(FakeEnding))]
    [HarmonyPatch("reconnectingScreen3_Accepted")]
    public static class SuccessReconnectPatch
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        //private static void reconnectingScreen3_Accepted(object sender, PlayerIndexEventArgs e)
        public static void Postfix(object sender, PlayerIndexEventArgs e)
        {
            try
            {
                _archipelago.ReconnectAfterTemporaryDisconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SuccessReconnectPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
