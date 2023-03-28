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
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        // protected void Complete()
        private static void Postfix(NISManager __instance)
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
    }
}
