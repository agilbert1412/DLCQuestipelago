using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib;
using HarmonyLib;

namespace DLCQuestipelago.DualContentManager
{
    [HarmonyPatch(typeof(DLCContentManager))]
    [HarmonyPatch(nameof(DLCContentManager.Initialize))]
    public static class DLCContentManagerInitializePatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoNotificationsHandler _notificationHandler;

        public static void Initialize(ManualLogSource log, ArchipelagoNotificationsHandler notificationsHandler)
        {
            _log = log;
            _notificationHandler = notificationsHandler;
        }

        //public void Initialize(IServiceProvider serviceProvider, string rootDirectory)
        private static void Postfix(DLCContentManager __instance, IServiceProvider serviceProvider, string rootDirectory)
        {
            try
            {
                Plugin.DualContentManager = new DLCDualContentManager(serviceProvider, rootDirectory, _log);
                Plugin.DualAssetManager = new DLCDualAssetManager(_log, Plugin.DualContentManager);
                _notificationHandler.LoadDlcPacks(Plugin.DualContentManager, Plugin.DualAssetManager);
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(DLCContentManagerInitializePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
