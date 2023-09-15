using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib.Save;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("GetSaveFilename")]
    public static class GetSaveFilenamePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
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
                var url = connectionInfo.HostUrl.Replace(":", "").Replace("/", "").Replace("\\", "");
                __result = $"{fileName}_{url}_{connectionInfo.Port}_{connectionInfo.SlotName}.{extension}";
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
