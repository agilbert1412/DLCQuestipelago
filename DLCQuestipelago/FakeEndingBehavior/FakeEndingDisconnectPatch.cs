using System;
using System.Diagnostics;
using DLCLib.Scripts.LFOD;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.FakeEndingBehavior
{
    [HarmonyPatch(typeof(FakeEnding))]
    [HarmonyPatch(nameof(FakeEnding.OnFakeEndingComplete))]
    public static class FakeEndingDisconnectPatch
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        //public static void OnFakeEndingComplete()
        public static void Postfix()
        {
            try
            {
                _archipelago.DisconnectTemporarily();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FakeEndingDisconnectPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
