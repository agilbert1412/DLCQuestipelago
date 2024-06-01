using System;
using System.Reflection;
using Core;
using DLCLib;
using DLCLib.Audio;
using DLCLib.DLC;
using DLCLib.Effects;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Items.Traps;

namespace DLCQuestipelago.Items
{
    public class TrapManager
    {
        public const string ZOMBIE_SHEEP = "Zombie Sheep";
        private const string TEMPORARY_SPIKE = "Temporary Spike";
        private const string LOADING_SCREEN = "Loading Screen";
        private const string NAME_CHANGE = "Name Change";

        public static readonly string[] TrapItems = new[] { ZOMBIE_SHEEP, TEMPORARY_SPIKE, LOADING_SCREEN, NAME_CHANGE };

        private static Scene CurrentScene => SceneManager.Instance.CurrentScene;
        private static Player Player => CurrentScene.Player;

        private readonly ArchipelagoClient _archipelago;
        private Random _random;

        public TrapManager(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
            _random = new Random();
        }

        public bool TryHandleTrap(string itemName, bool isNew)
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

            if (itemName == NAME_CHANGE)
            {
                DLCPurchaseEventUtil.PurchaseNameChange();
                return true;
            }

            return false;
        }

        public void SpawnRandomTrapOnPlayer()
        {
            var choice = _random.Next(0, 3);
            switch (choice)
            {
                case 0:
                    SpawnZombieSheepOnPlayer();
                    return;
                case 1:
                    SpawnTemporarySpikeAroundPlayer();
                    return;
                case 2:
                    TriggerLoadingScreen();
                    return;
            }
        }

        public static void SpawnZombieSheepOnPlayer()
        {
            var zombieSheep = new ZombieSheepTrap(Plugin.DualContentManager);
            zombieSheep.LoadContent();
            zombieSheep.Respawn(Player.Position);
            var addZombieSheepSpawningEffectMethod = typeof(EffectsManager).GetMethod("AddZombieSheepSpawningEffect",
                BindingFlags.Instance | BindingFlags.NonPublic);
            addZombieSheepSpawningEffectMethod.Invoke(CurrentScene.EffectsManager, new object[] { Player.Position });
        }

        private void SpawnTemporarySpikeAroundPlayer()
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
    }
}