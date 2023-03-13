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
    [HarmonyPatch("ClearSaveGameData")]
    public static class ClearSaveGamePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        static void Postfix(DLCSaveManager __instance, ref bool __result)
        {
            Plugin.Instance.SaveAndQuit();
        }
    }
}
