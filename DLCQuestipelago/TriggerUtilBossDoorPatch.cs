using System.Collections.Generic;
using BepInEx.Logging;
using DLCLib;
using DLCLib.Input;
using DLCLib.Screens;
using DLCLib.World;
using DLCQuestipelago.Archipelago;
using GameStateManagement;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;

namespace DLCQuestipelago
{
    [HarmonyPatch(typeof(TriggerUtil))]
    [HarmonyPatch("msgBox_AcceptedTakeCoins")]
    public static class TriggerUtilBossDoorPatch
    {
        private const string SWORD_1 = "Big Sword Pack";
        private const string SWORD_2 = "Really Big Sword Pack";
        private const string SWORD_3 = "Unfathomable Sword Pack";

        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago)
        {
            _log = log;
            _archipelago = archipelago;
        }

        //private static void msgBox_AcceptedTakeCoins(object sender, PlayerIndexEventArgs e)
        private static bool Prefix(object sender, PlayerIndexEventArgs e)
        {
            var hasSword1 = _archipelago.HasReceivedItem(SWORD_1, out _);
            var hasSword2 = _archipelago.HasReceivedItem(SWORD_2, out _);
            var hasSword3 = _archipelago.HasReceivedItem(SWORD_3, out _);
            var canEnter = hasSword1 && hasSword2 && hasSword3;

            if (canEnter)
            {
                return true; // run original logic
            }

            var firstMissingSword = hasSword1 ? (hasSword2 ? SWORD_3 : SWORD_2) : SWORD_1;
            MessageBoxScreen screen = new MessageBoxScreen($"Actually, I'll need you to come back later. I don't have {firstMissingSword} in stock yet.\n- Shopkeep");
            screen.SetInputs(new MenuInput(new List<Buttons>()
            {
                Buttons.A,
                Buttons.B,
                Buttons.Back
            }, new List<Keys>()
            {
                Keys.Space,
                Keys.Enter,
                Keys.Escape
            }, new List<InputAction>()
            {
                InputAction.Action,
                InputAction.Jump
            }, "Gah! Fine.", screen.OnAccept), null, null);
            DLCScreenManager.Instance.AddScreen(screen, DLCScreenManager.Instance.GetControllingPlayer());

            return false; // don't run original logic
        }
    }
}
