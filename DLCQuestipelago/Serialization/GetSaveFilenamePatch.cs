using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Save;
using DLCQuestipelago.Archipelago;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("GetSaveFilename")]
    public static class GetSaveFilenamePatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(GetSaveFilenamePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
