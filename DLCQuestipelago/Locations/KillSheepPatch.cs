using System;
using System.Collections.Generic;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib;
using DLCLib.Character;
using DLCQuestipelago.Extensions;
using DLCQuestipelago.Gifting;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.Locations
{
    [HarmonyPatch(typeof(Scene))]
    [HarmonyPatch("RecordMobDeath")]
    public static class KillSheepPatch
    {
        private static ManualLogSource _log;
        private static LocationChecker _locationChecker;
        private static GiftSender _giftSender;
        private static Dictionary<Vector2, int> _sheepIds;
        private static Dictionary<int, string> _sheepNames;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker, GiftSender giftSender)
        {
            _log = log;
            _locationChecker = locationChecker;
            _giftSender = giftSender;
            _sheepIds = new Dictionary<Vector2, int>
            {
                {new Vector2(177f, 29.5f), 0},
                {new Vector2(158f, 11f), 1},
                {new Vector2(217.5f, 3.5f), 2},
                {new Vector2(272f, 8f), 3},
                {new Vector2(273f, 24.5f), 4},
                {new Vector2(160.5f, 38f), 5},
                {new Vector2(157f, 48f), 6},
                {new Vector2(81f, 45f), 7},
                {new Vector2(54f, 22.5f), 8},
                {new Vector2(16.5f, 15f), 9},
                {new Vector2(19.5f, 19.5f), 10},
                {new Vector2(16.5f, 35.5f), 11},
                {new Vector2(107.5f, 26f), 12},
            };
            _sheepNames = new Dictionary<int, string>
            {
                {0, "Double Jump Alcove Sheep"},
                {1, "Double Jump Floating Sheep"},
                {2, "Sexy Outfits Sheep"},
                {3, "Forest High Sheep"},
                {4, "Forest Low Sheep"},
                {5, "Between Trees Sheep"},
                {6, "Hole in the Wall Sheep"},
                {7, "Shepherd Sheep"},
                {8, "Top Hat Sheep"},
                {9, "North West Ceiling Sheep"},
                {10, "North West Alcove Sheep"},
                {11, "West Cave Sheep"},
                {12, "Cutscene Sheep"},
            };
        }

        //internal void RecordMobDeath(Mob mob)
        public static bool Prefix(Scene __instance, Mob mob)
        {
            try
            {
                if (!(mob is Sheep sheep))
                {
                    return true; // run original logic;
                }

                var spawnPosition = mob.GetSpawnPosition();
                var id = _sheepIds[spawnPosition];
                var sheepLocationName = _sheepNames[id];
                _log.LogInfo($"SheepKilled Sheep #{id} at {spawnPosition} ({sheepLocationName})");
                _locationChecker.AddCheckedLocation(sheepLocationName);

                _giftSender.SendZombieSheepGift((int)spawnPosition.X, (int)spawnPosition.Y).FireAndForget();

                return true; // run original logic;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(KillSheepPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
