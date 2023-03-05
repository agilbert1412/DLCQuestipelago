using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using DLCLib.DLC;
using HarmonyLib;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(DLCPack))]
    [HarmonyPatch("Purchase")]
    public static class DLCPackPatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        static bool Prefix(DLCPack __instance)
        {
            _log.LogInfo($"Purchased a DLC! [{__instance.Data.DisplayName}]");
            return false; // don't run original logic
        }

        static void Postfix(DLCPack __instance)
        {

        }
    }
}
