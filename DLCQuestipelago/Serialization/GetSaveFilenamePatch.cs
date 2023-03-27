using DLCLib.Save;
using DLCQuestipelago.Archipelago;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("GetSaveFilename")]
    public static class GetSaveFilenamePatch
    {
        private static Logger _log;
        private static ArchipelagoClient _archipelagoClient;

        public static void Initialize(Logger log, ArchipelagoClient archipelagoClient)
        {
            _log = log;
            _archipelagoClient = archipelagoClient;
        }

        // protected string GetSaveFilename()
        private static void Postfix(DLCSaveManager __instance, ref string __result)
        {
            var connectionInfo = DLCQuestipelagoMod.Instance.APConnectionInfo;
            if (connectionInfo == null)
            {
                return;
            }
            __result = $"{__result}_{connectionInfo.HostUrl}_{connectionInfo.Port}_{connectionInfo.SlotName}";
        }
    }
}
