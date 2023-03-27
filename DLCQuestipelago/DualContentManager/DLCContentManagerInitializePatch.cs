using System;
using DLCLib;
using HarmonyLib;

namespace DLCQuestipelago.DualContentManager
{
    [HarmonyPatch(typeof(DLCContentManager))]
    [HarmonyPatch(nameof(DLCContentManager.Initialize))]
    public static class DLCContentManagerInitializePatch
    {
        private static Logger _log;
        private static ArchipelagoNotificationsHandler _notificationHandler;

        public static void Initialize(Logger log, ArchipelagoNotificationsHandler notificationsHandler)
        {
            _log = log;
            _notificationHandler = notificationsHandler;
        }

        //public void Initialize(IServiceProvider serviceProvider, string rootDirectory)
        private static void Postfix(DLCContentManager __instance, IServiceProvider serviceProvider, string rootDirectory)
        {
            DLCQuestipelagoMod.DualContentManager = new DLCDualContentManager(serviceProvider, rootDirectory, _log);
            DLCQuestipelagoMod.DualAssetManager = new DLCDualAssetManager(_log, DLCQuestipelagoMod.DualContentManager);
            _notificationHandler.LoadDlcPacks(DLCQuestipelagoMod.DualContentManager, DLCQuestipelagoMod.DualAssetManager);
        }
    }
}
