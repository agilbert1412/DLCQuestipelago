﻿using System;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using DLCLib;
using DLCLib.NIS;
using HarmonyLib;

namespace DLCQuestipelago.Archipelago.Deathlink
{
    [HarmonyPatch(typeof(NISManager))]
    [HarmonyPatch("Complete")]
    public static class RespawnAfterCutscenePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // protected void Complete()
        private static void Postfix(NISManager __instance)
        {
            try
            {
                var player = SceneManager.Instance.CurrentScene.Player;
                if (player.IsAlive)
                {
                    return;
                }

                var respawnPos = SceneManager.Instance.CurrentScene.CheckpointManager.GetRespawnPosition();
                var playerRespawnMethod =
                    typeof(Player).GetMethod("Respawn", BindingFlags.Instance | BindingFlags.NonPublic);
                playerRespawnMethod.Invoke(player, new object[] { respawnPos });
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(RespawnAfterCutscenePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
