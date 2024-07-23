using System;
using System.Diagnostics;
using DLCLib;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.DualContentManager
{
    [HarmonyPatch(typeof(DLCContentManager))]
    [HarmonyPatch(nameof(DLCContentManager.Initialize))]
    public static class DLCContentManagerInitializePatch
    {
        private static ILogger _logger;
        private static ArchipelagoNotificationsHandler _notificationHandler;

        public static void Initialize(ILogger logger, ArchipelagoNotificationsHandler notificationsHandler)
        {
            _logger = logger;
            _notificationHandler = notificationsHandler;
        }

        //public void Initialize(IServiceProvider serviceProvider, string rootDirectory)
        private static void Postfix(DLCContentManager __instance, IServiceProvider serviceProvider, string rootDirectory)
        {
            try
            {
                Plugin.DualContentManager = new DLCDualContentManager(serviceProvider, rootDirectory, _logger);
                Plugin.DualAssetManager = new DLCDualAssetManager(_logger, Plugin.DualContentManager);
                _notificationHandler.LoadDlcPacks(Plugin.DualContentManager, Plugin.DualAssetManager);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DLCContentManagerInitializePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
