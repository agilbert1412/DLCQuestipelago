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
    [HarmonyPatch("reconnectingScreen_Accepted")]
    public static class FailedReconnectPatch
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        //private static void reconnectingScreen_Accepted(object sender, PlayerIndexEventArgs e)
        public static void Postfix(object sender, PlayerIndexEventArgs e)
        {
            try
            {
                _archipelago.MakeSureConnected(0);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FailedReconnectPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
