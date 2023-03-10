using System.IO;
using System.Reflection;
using BepInEx.Logging;
using DLCLib.Save;
using EasyStorage;
using HarmonyLib;
using Newtonsoft.Json;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(DLCSaveManager))]
    [HarmonyPatch("SaveGameData")]
    public static class SaveGameDataPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        static void Postfix(DLCSaveManager __instance, ref bool __result)
        {
            if (!Directory.Exists(Persistency.SaveDirectory))
            {
                Directory.CreateDirectory(Persistency.SaveDirectory);
            }

            Plugin.Instance.OnSaving();
            var serialized = JsonConvert.SerializeObject(Plugin.Instance.ArchipelagoState);
            File.WriteAllText(Persistency.SaveFile, serialized);
        }
    }
}
