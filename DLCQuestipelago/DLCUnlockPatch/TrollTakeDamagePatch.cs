using DLCLib.Character;
using DLCLib.DLC;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DLCQuestipelago.DLCUnlockPatch
{
    [HarmonyPatch(typeof(TrollNPC))]
    [HarmonyPatch(nameof(TrollNPC.TakeDamage))]
    public static class TrollTakeDamagePatch
    {
        private static Logger _log;

        public static void Initialize(Logger log)
        {
            _log = log;
        }

        // public override void TakeDamage(int damageAmount, Vector2 direction)
        private static void Postfix(TrollNPC __instance, int damageAmount, Vector2 direction)
        {
            DLCManager.Instance.UnlockPack("gun");
        }
    }
}
