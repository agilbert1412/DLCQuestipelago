using DLCLib.Campaigns;
using DLCLib.Character;
using DLCLib.DLC;
using DLCQuestipelago.Archipelago;
using System.Collections.Generic;
using System.Reflection;
using Awardments;
using DLCLib;
using DLCLib.Conversation;

namespace DLCQuestipelago.Items
{
    public class ItemParser
    {
        private ArchipelagoClient _archipelago;
        private readonly TrapManager _trapManager;
        public HashSet<string> ReceivedDLCs { get; }

        public ItemParser(ArchipelagoClient archipelago, TrapManager trapManager)
        {
            ReceivedDLCs = new HashSet<string>();
            _archipelago = archipelago;
            _trapManager = trapManager;
        }

        private static Scene CurrentScene => SceneManager.Instance.CurrentScene;
        private static Player Player => CurrentScene.Player;

        private static MethodInfo ZeldaMethod = typeof(Player).GetMethod("PerformZeldaItem", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string), typeof(string), typeof(float)}, null);

        public void ProcessItem(ReceivedItem item, bool isNew)
        {
            ProcessItem(item.ItemName, isNew);
        }

        public void ProcessItem(string itemName, bool isNew)
        {
            if (TryHandleDLC(itemName, isNew))
            {
                return;
            }

            if (TryHandleItem(itemName, isNew))
            {
                return;
            }

            if (_trapManager.TryHandleTrap(itemName, isNew))
            {
                return;
            }
        }

        private bool TryHandleDLC(string itemName, bool isNew)
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
                if (ReceivedDLCs.Contains(unlockedDLC.Data.Name))
                {
                    return true;
                }

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

        private static bool TryHandleItem(string itemName, bool isNew)
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
            var fetchNpc = CurrentScene.NPCManager.GetNPCByName("fetch") as FetchNPC;
            if (CurrentScene.EventList.Contains(ConversationManager.FETCH_QUEST_ACTIVATED_STR) && fetchNpc != null && fetchNpc.FetchQuestStarted)
            {
                CurrentScene.HUDManager.ObjectiveDisplay.MarkObjectivesComplete();
            }
        }

        private static void GivePlayerBindle()
        {
            Player.Inventory.HasBindle = true;
            ZeldaMethod.Invoke(Player, new object[] { "bindle", "Humble Indie Bindle", 1f });
        }
    }
}