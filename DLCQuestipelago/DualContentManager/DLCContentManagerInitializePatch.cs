using System;
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

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        //public void Initialize(IServiceProvider serviceProvider, string rootDirectory)
        private static void Postfix(DLCContentManager __instance, IServiceProvider serviceProvider, string rootDirectory)
        {
            Plugin.DualContentManager = new DLCDualContentManager(serviceProvider, rootDirectory, _log);
            Plugin.DualAssetManager = new DLCDualAssetManager(_log, Plugin.DualContentManager);
        }
    }
}
