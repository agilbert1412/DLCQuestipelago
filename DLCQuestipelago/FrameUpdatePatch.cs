using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(DLCGame.DLCGame))]
    [HarmonyPatch("Update")]
    public static class FrameUpdatePatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        // protected override void Update(GameTime gameTime)
        private static void Postfix(DLCGame.DLCGame __instance, GameTime gameTime)
        {
            DLCQuestipelagoMod.Instance.OnUpdateTicked(gameTime);
        }
    }
}
