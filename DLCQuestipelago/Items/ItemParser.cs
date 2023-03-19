using System;
using DLCLib.Campaigns;
using DLCLib.Character;
using DLCLib.DLC;
using DLCQuestipelago.Archipelago;
using System.Collections.Generic;
using System.Reflection;
using Awardments;
using Core;
using DLCLib;
using DLCLib.Conversation;

namespace DLCQuestipelago.Items
{
    public class ItemParser
    {
        public HashSet<string> ReceivedDLCs { get; }

        public ItemParser()
        {
            ReceivedDLCs = new HashSet<string>();
        }

        private static Scene CurrentScene => SceneManager.Instance.CurrentScene;
        private static Player Player => CurrentScene.Player;

        private static MethodInfo ZeldaMethod = typeof(Player).GetMethod("PerformZeldaItem", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string), typeof(string), typeof(float)}, null);

        public void ProcessItem(ReceivedItem item)
        {
            if (TryHandleDLC(item.ItemName))
            {
                return;
            }

            if (TryHandleItem(item.ItemName))
            {
                return;
            }
        }

        private bool TryHandleDLC(string itemName)
        {
            DLCPack unlockedDLC = null;
            foreach (var x in DLCManager.Instance.Packs)
            {
                if (x.Value.Data.DisplayName == itemName)
                {
                    unlockedDLC = x.Value;
                    break;
                }
            }

            if (unlockedDLC != null)
            {
                ReceivedDLCs.Add(unlockedDLC.Data.Name);
                if (unlockedDLC.Data.PurchaseEvent == null || unlockedDLC.Data.PurchaseEvent.Equals(string.Empty) ||
                    unlockedDLC.Data.IsBossDLC)
                {
                    return true;
                }

                typeof(DLCPurchaseEventUtil).InvokeMember(unlockedDLC.Data.PurchaseEvent, BindingFlags.InvokeMethod, null, null,
                    new object[0]);
                return true;
            }

            return false;
        }

        private static bool TryHandleItem(string itemName)
        {
            if (CampaignManager.Instance.Campaign is DLCQuestCampaign)
            {
                if (itemName == "Sword")
                {
                    GivePlayerDlcQuestSword();
                    return true;
                }

                if (itemName == "Gun")
                {
                    GivePlayerGun();
                    return true;
                }
            }
            else
            {
                if (itemName == "Wooden Sword")
                {
                    GivePlayerWoodenSword();
                    return true;
                }

                if (itemName == "Box of Various Supplies")
                {
                    GivePlayerBoxOfSupplies();
                    return true;
                }

                if (itemName == "Humble Indie Bindle")
                {
                    GivePlayerBindle();
                    return true;
                }

                if (itemName == "Pickaxe")
                {
                    GrooveNPC.GiveMattock(true);
                    return true;
                }
            }

            return false;
        }

        private static void GivePlayerDlcQuestSword()
        {
            Player.Inventory.HasSword = true;
            Player.RefreshAnimations();
        }

        private static void GivePlayerGun()
        {
            Player.Inventory.HasGun = true;
            Player.RefreshAnimations();
            AwardmentManager.Instance.Award("packingheat");
        }

        private static void GivePlayerWoodenSword()
        {
            Player.Inventory.HasSword = true;
            Player.RefreshAnimations();
            ZeldaMethod.Invoke(Player, new object[] { "boss_sword_0", "Sword", 2f });
            AwardmentManager.Instance.Award("storyprogress");
        }

        private static void GivePlayerBoxOfSupplies()
        {
            ZeldaMethod.Invoke(Player, new object[] { "box_of_supplies", "Box of Various Supplies", 1f });
            CurrentScene.EventList.Add(FetchNPC.FETCH_COLLECTED_STR);
            if (CurrentScene.EventList.Contains(ConversationManager.FETCH_QUEST_ACTIVATED_STR) && ((FetchNPC)CurrentScene.NPCManager.GetNPCByName("fetch")).FetchQuestStarted)
            {
                CurrentScene.HUDManager.ObjectiveDisplay.MarkObjectivesComplete();
            }
        }

        private static void GivePlayerBindle()
        {
            Player.Inventory.HasBindle = true;
            ZeldaMethod.Invoke(Player, new object[] { "bindle", "Humble Indie Bindle", 1f });
        }

        /*
        private FieldInfo _isZeldaField = typeof(Player).GetField("isZelda", BindingFlags.Instance)
        private static void PerformZeldaItem(Player player, string textureName, string message, float scale)
        {
            if (player.isZelda || !player.AllowPerformZeldaItem)
                return;
            player.isZelda = true;
            player.OverrideAnimation("zelda");
            player.zeldaItem.Play(Singleton<SceneManager>.Instance.CurrentScene.AssetManager.CampaignObjectSpriteSheet, textureName, message, Player.ZELDA_ITEM_TIME, scale);
            Singleton<SceneManager>.Instance.CurrentScene.HUDManager.ShowHUD = false;
            Singleton<SceneManager>.Instance.CurrentScene.Timers.Create(0.1f, false, (Action<Timer>)(delay => DLCAudioManager.Instance.PlaySound("zelda_item")));
            player.zeldaTimer = Singleton<SceneManager>.Instance.CurrentScene.Timers.Create(Player.ZELDA_ITEM_TIME, false, (Action<Timer>)(finish =>
            {
                player.zeldaTimer = (Timer)null;
                this.FinishZeldaItem();
            }));
        }

        private static void FinishZeldaItem(Player player)
        {
            player.isZelda = false;
            player.ResetAnimation();
            Singleton<SceneManager>.Instance.CurrentScene.HUDManager.ShowHUD = true;
        }*/
    }
}