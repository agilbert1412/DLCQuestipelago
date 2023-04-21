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

                __result = $"{__result}_{connectionInfo.HostUrl}_{connectionInfo.Port}_{connectionInfo.SlotName}";
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
