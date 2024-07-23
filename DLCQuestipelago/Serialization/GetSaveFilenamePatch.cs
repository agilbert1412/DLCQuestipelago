using System;
using System.Diagnostics;
using DLCLib.Save;
using DLCQuestipelago.Archipelago;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("GetSaveFilename")]
    public static class GetSaveFilenamePatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // protected string GetSaveFilename()
        private static void Postfix(DLCSaveManager __instance, ref string __result)
        {
            try
            {
                var connectionInfo = Plugin.Instance.APConnectionInfo;
                if (connectionInfo == null)
                {
                    return;
                }

                var parts = __result.Split('.');
                var fileName = parts[0];
                var extension = parts.Length > 1 ? parts[1] : ".xml";
                __result = $"{fileName}_{connectionInfo.Port}_{connectionInfo.SlotName}_{_archipelago.SlotData.Seed}.{extension}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetSaveFilenamePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
