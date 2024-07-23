using DLCLib.Campaigns;
using DLCLib.Character;
using DLCLib.DLC;
using System.Collections.Generic;
using System.Reflection;
using Awardments;
using DLCLib;
using DLCLib.Conversation;
using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace DLCQuestipelago.Items
{
    public class ItemParser
    {
        private const string DLC_QUEST_WEAPON = "DLC Quest: Progressive Weapon";
        private const string LFOD_WEAPON = "Live Freemium or Die: Progressive Weapon";

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
            if (TryHandleDLC(itemName))
            {
                return;
            }

            if (TryHandleItem(itemName))
            {
                return;
            }

            if (_trapManager.TryHandleTrap(itemName, isNew))
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
                if (ReceivedDLCs.Contains(unlockedDLC.Data.Name))
                {
                    if (itemName.Contains("Name Change"))
                    {
                        DLCPurchaseEventUtil.PurchaseNameChange();
                    }
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

        private bool TryHandleItem(string itemName)
        {
            if (CampaignManager.Instance.Campaign is DLCQuestCampaign)
            {
                if (itemName == DLC_QUEST_WEAPON)
                {
                    SetCorrectDlcQuestWeapon();
                    return true;
                }

                if (itemName == "Sword")
                {
                    SetPlayerDlcQuestSword(true);
                    return true;
                }

                if (itemName == "Gun")
                {
                    SetPlayerGun(true);
                    return true;
                }
            }
            else
            {
                if (itemName == LFOD_WEAPON)
                {
                    SetCorrectLFODWeapon();
                    return true;
                }

                if (itemName == "Wooden Sword")
                {
                    SetPlayerWoodenSword(true);
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
                    SetPlayerPickaxe(true);
                    return true;
                }
            }

            return false;
        }

        private static void SetPlayerDlcQuestSword(bool hasSword)
        {
            Player.Inventory.HasSword = hasSword;
            Player.RefreshAnimations();
        }

        private static void SetPlayerGun(bool hasGun)
        {
            Player.Inventory.HasGun = hasGun;
            Player.RefreshAnimations();
            if (hasGun)
            {
                AwardmentManager.Instance.Award("packingheat");
            }
        }

        private void SetCorrectDlcQuestWeapon()
        {
            var numWeaponsReceived = _archipelago.GetReceivedItemCount(DLC_QUEST_WEAPON);
            switch (numWeaponsReceived)
            {
                case <= 0:
                    SetPlayerDlcQuestSword(false);
                    SetPlayerGun(false);
                    return;
                case 1:
                    SetPlayerDlcQuestSword(true);
                    SetPlayerGun(false);
                    return;
                default:
                    SetPlayerDlcQuestSword(true);
                    SetPlayerGun(true);
                    break;
            }
        }

        private static void SetPlayerWoodenSword(bool hasWoodenSword)
        {
            Player.Inventory.HasSword = hasWoodenSword;
            Player.RefreshAnimations();
            if (hasWoodenSword)
            {
                ZeldaMethod.Invoke(Player, new object[] { "boss_sword_0", "Sword", 2f });
                AwardmentManager.Instance.Award("storyprogress");
            }
        }

        private static void SetPlayerPickaxe(bool hasPickaxe)
        {
            if (hasPickaxe)
            {
                GrooveNPC.GiveMattock(true);
            }
            else
            {
                Player.Inventory.HasMattock = false;
                Player.AttachedWeapon = null;
                Player.RefreshAnimations();
            }
        }

        private void SetCorrectLFODWeapon()
        {
            var numWeaponsReceived = _archipelago.GetReceivedItemCount(LFOD_WEAPON);
            switch (numWeaponsReceived)
            {
                case <= 0:
                    SetPlayerWoodenSword(false);
                    SetPlayerPickaxe(false);
                    return;
                case 1:
                    SetPlayerWoodenSword(true);
                    SetPlayerPickaxe(false);
                    return;
                default:
                    SetPlayerWoodenSword(true);
                    SetPlayerPickaxe(true);
                    break;
            }
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