using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Archipelago;
using GameStateManagement;
using HarmonyLib;

namespace DLCQuestipelago.FakeEndingBehavior
{
    [HarmonyPatch(typeof(FakeEnding))]
    [HarmonyPatch("reconnectingScreen3_Accepted")]
    public static class SuccessReconnectPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(SuccessReconnectPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
