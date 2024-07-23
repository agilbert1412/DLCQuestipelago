using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
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
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
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
                _logger.LogError($"Failed in {nameof(ContinueGamePatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }
    }
}
