using BepInEx.Logging;
using DLCLib;
using DLCLib.World.Props;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(DLCGame.DLCGame))]
    [HarmonyPatch("Update")]
    public static class FrameUpdatePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        // protected override void Update(GameTime gameTime)
        static void Postfix(DLCGame.DLCGame __instance, GameTime gameTime)
        {
            Plugin.Instance.OnUpdateTicked();
        }
    }
}
