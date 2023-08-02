using DLCLib.Campaigns;
using DLCLib.Character;
using DLCLib.DLC;
using DLCQuestipelago.Archipelago;
using System.Collections.Generic;
using System.Reflection;
using Awardments;
using Core;
using DLCLib;
using DLCLib.Audio;
using DLCLib.Conversation;
using DLCLib.Effects;
using DLCLib.World.Props;
using DLCQuestipelago.Items.Traps;

namespace DLCQuestipelago.Items
{
    public class ItemParser
    {
        public const string ZOMBIE_SHEEP = "Zombie Sheep";
        private const string TEMPORARY_SPIKE = "Temporary Spike";
        private const string LOADING_SCREEN = "Loading Screen";

        public static readonly string[] TrapItems = new[] { ZOMBIE_SHEEP, TEMPORARY_SPIKE, LOADING_SCREEN };

        private ArchipelagoClient _archipelago;
        public HashSet<string> ReceivedDLCs { get; }

        public ItemParser(ArchipelagoClient archipelago)
        {
            ReceivedDLCs = new HashSet<string>();
            _archipelago = archipelago;
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

            if (TryHandleTrap(itemName, isNew))
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

        private bool TryHandleTrap(string itemName, bool isNew)
        {
            if (!isNew)
            {
                return false;
            }

            if (itemName == ZOMBIE_SHEEP)
            {
                SpawnZombieSheepOnPlayer();
                return true;
            }
            if (itemName == TEMPORARY_SPIKE)
            {
                SpawnTemporarySpikeAroundPlayer();
                return true;
            }
            if (itemName == LOADING_SCREEN)
            {
                TriggerLoadingScreen();
                return true;
            }

            return false;
        }

        public static void SpawnZombieSheepOnPlayer()
        {
            var zombieSheep = new ZombieSheepTrap(Plugin.DualContentManager);
            zombieSheep.LoadContent();
            zombieSheep.Respawn(Player.Position);
            var addZombieSheepSpawningEffectMethod =
                typeof(EffectsManager).GetMethod("AddZombieSheepSpawningEffect",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            addZombieSheepSpawningEffectMethod.Invoke(CurrentScene.EffectsManager, new object[] { Player.Position });
        }

        private static void SpawnTemporarySpikeAroundPlayer()
        {
            var spikePosition = Player.Position;
            var spike = new TemporarySpike(spikePosition, Spike.DirectionEnum.Up, 3);
            spike.LoadContent();
            var delay = CurrentScene.IsBossMode ? 8f : 3f;
            CurrentScene.Interpolators.Create(0.0f, 1f, delay, null, (_) => CurrentScene.AddToScene(spike));
        }

        private void TriggerLoadingScreen()
        {
            DLCAudioManager.Instance.AudioSystem.PauseMusic();
            CurrentScene.IsLoading = true;
            CurrentScene.IsPauseAllowed = false;
            CurrentScene.HUDManager.LoadingDisplay.Visible = true;
            CurrentScene.HUDManager.LoadingDisplay.HandleProgressChanged(0.0f);
            var length = _archipelago.HasReceivedItem("Day One Patch Pack", out _) ? 2f : 5f;
            CurrentScene.Interpolators.Create(0.0f, 1f, length, step => CurrentScene.HUDManager.LoadingDisplay.HandleProgressChanged(step.Value), FinishLoadingScreen);
        }

        private static void FinishLoadingScreen(Interpolator interpolator)
        {
            DLCAudioManager.Instance.AudioSystem.ResumeMusic();
            CurrentScene.IsLoading = false;
            CurrentScene.IsPauseAllowed = true;
            CurrentScene.HUDManager.LoadingDisplay.Visible = false;
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