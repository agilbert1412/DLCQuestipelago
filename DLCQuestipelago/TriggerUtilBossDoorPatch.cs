using System;
using System.Collections.Generic;
using System.Diagnostics;
using DLCLib;
using DLCLib.Input;
using DLCLib.Screens;
using DLCLib.World;
using GameStateManagement;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
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


        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        //private static void msgBox_AcceptedTakeCoins(object sender, PlayerIndexEventArgs e)
        private static bool Prefix(object sender, PlayerIndexEventArgs e)
        {
            try
            {
                var hasSword1 = HasItemOrWillHaveItInSpecificLocations(SWORD_1, new[] { SWORD_1 });
                var hasSword2 = HasItemOrWillHaveItInSpecificLocations(SWORD_2, new[] { SWORD_1, SWORD_2 });
                var hasSword3 = HasItemOrWillHaveItInSpecificLocations(SWORD_3, new[] { SWORD_1, SWORD_2, SWORD_3 });
                var canEnter = hasSword1 && hasSword2 && hasSword3;

                if (canEnter)
                {
                    return true; // run original logic
                }

                var firstMissingSword = hasSword1 ? (hasSword2 ? SWORD_3 : SWORD_2) : SWORD_1;
                MessageBoxScreen screen = new MessageBoxScreen(
                    $"Actually, I'll need you to come back later. I don't have {firstMissingSword} in stock yet.\n- Shopkeep");
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
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TriggerUtilBossDoorPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }

        private static bool HasItemOrWillHaveItInSpecificLocations(string item, IEnumerable<string> validLocations)
        {
            var hasItem = _archipelago.HasReceivedItem(item);
            if (hasItem)
            {
                return true;
            }

            foreach (var validLocation in validLocations)
            {
                var scoutedLocation = _archipelago.ScoutSingleLocation(validLocation);
                if (scoutedLocation == null)
                {
                    continue;
                }

                if (scoutedLocation.ItemName == item)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
