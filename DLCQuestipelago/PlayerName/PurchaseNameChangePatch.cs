using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using DLCLib;
using DLCLib.DLC;
using DLCLib.Input;
using DLCLib.Screens;
using DLCQuestipelago.Archipelago;
using GameStateManagement;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DLCQuestipelago.PlayerName
{
    [HarmonyPatch(typeof(DLCPurchaseEventUtil))]
    [HarmonyPatch(nameof(DLCPurchaseEventUtil.PurchaseNameChange))]
    public static class PurchaseNameChangePatch
    {
        private static string NEW_NAME_1 = "{0} 2";
        private static string NEW_NAME_2 = "Axe_y";

        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static NameChanger _nameChanger;

        private static string ApName => _archipelago.GetPlayerAlias(_archipelago.SlotData.SlotName) ?? _archipelago.SlotData.SlotName;
        private static string NewName1 => string.Format(NEW_NAME_1, ApName);
        private static string NewName2 => NEW_NAME_2;
        private static string FinalName => _nameChanger.ChangeName(ApName);

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, NameChanger nameChanger)
        {
            _log = log;
            _archipelago = archipelago;
            _nameChanger = nameChanger;
        }

        //public static void PurchaseNameChange()
        private static bool Prefix()
        {
            Console.WriteLine("Name Change Pack purchased!");
            if (!SceneManager.Instance.CurrentScene.Player.AllowPerformZeldaItem)
            {
                return false;
            }

            var newName = NewName1;
            var promptPrefix = "Name Change purchased!";
            CreateAndAnimateNameChangePrompt(promptPrefix, newName, NameChangeFailed_Accepted);
            return false;
        }

        private static void NameChangeFailed_Accepted(object sender, PlayerIndexEventArgs e)
        {
            var newName = NewName2;
            var promptPrefix = NewName1 + " is already taken!";
            CreateAndAnimateNameChangePrompt(promptPrefix, newName, NameChangeFailed2_Accepted);
        }

        private static void NameChangeFailed2_Accepted(object sender, PlayerIndexEventArgs e)
        {
            var newName = FinalName;
            var promptPrefix = NewName2 + " is already taken!";
            CreateAndAnimateNameChangePrompt(promptPrefix, newName, NameChangeFinal_Accepted);
        }

        private static void NameChangeFinal_Accepted(object sender, PlayerIndexEventArgs e)
        {
            var screen = new MessageBoxScreen("Name change accepted!\n\nWelcome #" +
                                              FinalName + "#\n");
            _archipelago.SendMessage($"!alias {FinalName}");
            screen.SetInputs(null, new MenuInput(new List<Buttons>()
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
            }, "Fine", screen.OnAccept), null);
            DLCScreenManager.Instance.AddScreen(screen, DLCScreenManager.Instance.GetControllingPlayer());
        }

        private static void CreateAndAnimateNameChangePrompt(string promptPrefix, string newName, EventHandler<PlayerIndexEventArgs> actionAfter)
        {
            var prompt = promptPrefix + "\nPlease enter a new name:\n";
            var parameteredPrompt = promptPrefix + "\nPlease enter a new name:\n#{0}#";
            var screen = new ReconnectingScreen(prompt, newName.Length + 16);
            screen.SetInputs(null, null, null);
            screen.Accepted += actionAfter;
            screen.TotalTime = 6f;
            screen.OnTick += ticknum =>
            {
                if (ticknum > newName.Length + 8)
                    return prompt;
                var typedPortionOfName = ticknum > 8 ? newName.Substring(0, newName.Length - ticknum + 8) : newName;
                return string.Format(parameteredPrompt, typedPortionOfName);
            };
            DLCScreenManager.Instance.AddScreen(screen, DLCScreenManager.Instance.GetControllingPlayer());
        }
    }
}
