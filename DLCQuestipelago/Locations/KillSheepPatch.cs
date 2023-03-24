using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using DLCLib;
using DLCLib.Character;
using DLCLib.Render;
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
        private static Dictionary<Vector2, int> _sheepIds;
        private static Dictionary<int, string> _sheepNames;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
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
                {0, "Sheep"},
                {1, "Sheep"},
                {2, "Sheep"},
                {3, "Sheep"},
                {4, "Sheep"},
                {5, "Sheep"},
                {6, "Sheep"},
                {7, "Sheep"},
                {8, "Sheep"},
                {9, "Sheep"},
                {10, "Sheep"},
                {11, "Sheep"},
                {12, "Sheep"},
            };
        }

        //internal void RecordMobDeath(Mob mob)
        public static bool Prefix(Scene __instance, Mob mob)
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
            return true; // run original logic;
        }
    }
}
