using System;
using System.Diagnostics;
using BepInEx.Logging;
using DLCLib;
using DLCLib.Campaigns;
using DLCLib.Screens;
using GameStateManagement;
using HarmonyLib;

namespace DLCQuestipelago.Serialization
{
    [HarmonyPatch(typeof(MainMenuScreen))]
    [HarmonyPatch("continueGameEntry_Selected")]
    public static class ContinueGamePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // private void continueGameEntry_Selected(object sender, PlayerIndexEventArgs e)
        private static void Postfix(MainMenuScreen __instance, object sender, PlayerIndexEventArgs e)
        {
            try
            {
                if (Plugin.Instance.IsInGame)
                {
                    return;
                }

                Plugin.Instance.EnterGame();

                if (!CampaignManager.Instance.Campaign.SpawnOnlyAtCheckpoint)
                {
                    return;
                }

                var scene = SceneManager.Instance?.CurrentScene;
                if (scene == null)
                {
                    return;
                }

                scene.Player.Teleport(scene.CheckpointManager.GetRespawnPosition());
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(ContinueGamePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
