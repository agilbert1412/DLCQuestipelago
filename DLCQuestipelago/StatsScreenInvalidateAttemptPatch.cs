using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.DLC;
using DLCLib.HUD;
using DLCLib.Screens;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using HarmonyLib;
using HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(StatsScreen))]
    [HarmonyPatch("IsValidAttempt")]
    public static class StatsScreenInvalidateAttemptPatch
    {
        private static ManualLogSource _log;
        private static LocationChecker _locationChecker;
        private static ItemParser _itemParser;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker, ItemParser itemParser)
        {
            _log = log;
            _locationChecker = locationChecker;
            _itemParser = itemParser;
        }

        // protected bool IsValidAttempt()
        public static bool Prefix(StatsScreen __instance, ref bool __result)
        {
            __result = false;
            return false; // don't run original logic
        }
    }
}
