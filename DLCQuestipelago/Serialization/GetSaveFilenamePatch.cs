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
        private static ArchipelagoClient _archipelagoClient;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelagoClient)
        {
            _log = log;
            _archipelagoClient = archipelagoClient;
        }

        // protected string GetSaveFilename()
        private static void Postfix(DLCSaveManager __instance, ref string __result)
        {
            var connectionInfo = Plugin.Instance.APConnectionInfo;
            if (connectionInfo == null)
            {
                return;
            }
            __result = $"{__result}_{connectionInfo.HostUrl}_{connectionInfo.Port}_{connectionInfo.SlotName}";
        }
    }
}
