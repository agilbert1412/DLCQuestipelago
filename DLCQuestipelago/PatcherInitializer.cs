using BepInEx.Logging;
using DLCQuestipelago.AntiCrashes;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Archipelago.Deathlink;
using DLCQuestipelago.Coins;
using DLCQuestipelago.DLCUnlockPatch;
using DLCQuestipelago.DualContentManager;
using DLCQuestipelago.FakeEndingBehavior;
using DLCQuestipelago.Gifting;
using DLCQuestipelago.Gifting.Patches;
using DLCQuestipelago.Items;
using DLCQuestipelago.Items.Traps;
using DLCQuestipelago.ItemShufflePatches;
using DLCQuestipelago.Locations;
using DLCQuestipelago.PlayerName;
using DLCQuestipelago.Serialization;
using DLCQuestipelago.Shop;

namespace DLCQuestipelago
{
    public static class PatcherInitializer
    {
        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker, ItemManager itemManager, ObjectivePersistence objectivePersistence, GiftSender giftSender)
        {
            InitializeDualContentManagerPatches(log);
            FrameUpdatePatch.Initialize(log);
            StatsScreenInvalidateAttemptPatch.Initialize(log);
            DLCPackPurchasePatch.Initialize(log, locationChecker);
            DLCIsPurchasedPatch.Initialize(log, itemManager.ItemParser);
            InventoryMakePurchasePatch.Initialize(log, archipelago);
            StoreScreenSelectionChangedPatch.Initialize(log, archipelago);
            CoinPickupPatch.Initialize(log, archipelago, locationChecker);
            InventoryCoinsGetPatch.Initialize(log, archipelago);
            HandleCoinChangedPatch.Initialize(log, archipelago);
            GrooveGiveMattockPatch.Initialize(log, locationChecker);
            StoreScreenSetupEntriesPatch.Initialize(log, locationChecker, itemManager.ItemParser);
            TriggerUtilBossDoorPatch.Initialize(log, archipelago);
            BossSheepAttackPatch.Initialize(log, archipelago);
            DiePatch.Initialize(log, archipelago);
            InitializeItemShufflePatches(log, archipelago, locationChecker);
            InitializeDLCUnlockPatches(log, archipelago, locationChecker);
            InitializeAllObjectivePatches(log, objectivePersistence);
            InitializeFakeEndingDisconnectPatches(log, archipelago);
            InitializeNameChangePatches(log, archipelago);
            InitializeAllPersistencyPatches(log, archipelago);
            InitializeKillAndDestroyPatches(log, locationChecker, giftSender);
            InitializeAwardmentPatches(log, locationChecker);
            InitializeAntiCrashPatches(log);
            CoinsAppearancePatch.ReplaceBrokenCoins(log, archipelago);
        }

        private static void InitializeKillAndDestroyPatches(ManualLogSource log, LocationChecker locationChecker,
            GiftSender giftSender)
        {
            DestroyBushPatch.Initialize(log, giftSender);
            DestroyRockPatch.Initialize(log, giftSender);
            DestroyVinePatch.Initialize(log, giftSender);
            KillSheepPatch.Initialize(log, locationChecker, giftSender);
        }

        private static void InitializeDualContentManagerPatches(ManualLogSource log)
        {
            ConstructAnimationPatch.Initialize(log);
        }

        private static void InitializeItemShufflePatches(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            CompleteFetchQuestPatch.Initialize(log, archipelago, locationChecker);
            GetPickaxePatch.Initialize(log, archipelago, locationChecker);
            GrantGunPatch.Initialize(log, archipelago, locationChecker);
            GrantSwordPatch.Initialize(log, archipelago, locationChecker);
            GrantWoodenSwordPatch.Initialize(log, archipelago, locationChecker);
            PickupBoxOfSuppliesPatch.Initialize(log, archipelago, locationChecker);
            RockDestructionPatch.Initialize(log);
            var conversationStarter = new ConversationStarter();
            MiddleAgeNpcActivatePatch.Initialize(log, archipelago, locationChecker, conversationStarter);
            GrooveNpcActivatePatch.Initialize(log, archipelago, locationChecker, conversationStarter);
            FetchNpcActivatePatch.Initialize(log, archipelago, locationChecker, conversationStarter);
        }

        private static void InitializeDLCUnlockPatches(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            GrindstoneUnlockPackPatch.Initialize(log, archipelago, locationChecker);
            RandomEncounterUnlockPackPatch.Initialize(log);
            TriggerUtilActivateForestPatch.Initialize(log);
            TriggerUtilActivateNightForestPatch.Initialize(log);
            TriggerUtilGameEndingPatch.Initialize(log);
            ZoneOnEnterPatch.Initialize(log);
            ExamineBanWallPatch.Initialize(log);
            ComedianDLCPatch.Initialize(log);
        }

        private static void InitializeAllObjectivePatches(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            InitializeDlcQuestObjectivePatches(log, objectivePersistence);
            InitializeLfodObjectivePatches(log, objectivePersistence);
        }

        private static void InitializeDlcQuestObjectivePatches(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            Goal.DLCQuest.FakeEndingObjectivePatch.Initialize(log, objectivePersistence);
            Goal.DLCQuest.BadEndingObjectivePatch.Initialize(log, objectivePersistence);
            Goal.DLCQuest.GoodEndingObjectivePatch.Initialize(log, objectivePersistence);
        }

        private static void InitializeLfodObjectivePatches(ManualLogSource log, ObjectivePersistence objectivePersistence)
        {
            Goal.LFOD.FakeEndingObjectivePatch.Initialize(log, objectivePersistence);
            Goal.LFOD.TrueEndingObjectivePatch.Initialize(log, objectivePersistence);
        }

        private static void InitializeFakeEndingDisconnectPatches(ManualLogSource log, ArchipelagoClient archipelago)
        {
            FakeEndingDisconnectPatch.Initialize(log, archipelago);
            FailedReconnectPatch.Initialize(log, archipelago);
            SuccessReconnectPatch.Initialize(log, archipelago);
        }

        private static void InitializeNameChangePatches(ManualLogSource log, ArchipelagoClient archipelago)
        {
            var nameChanger = new NameChanger(archipelago);
            PlayerNameInDialogPatch.Initialize(log, archipelago, nameChanger);
            PlayerNameInMessageBoxPatch.Initialize(log, archipelago, nameChanger);
            PurchaseNameChangePatch.Initialize(log, archipelago, nameChanger);
        }

        private static void InitializeAllPersistencyPatches(ManualLogSource log, ArchipelagoClient archipelago)
        {
            ClearSaveGamePatch.Initialize(log);
            ContinueGamePatch.Initialize(log);
            GetSaveFilenamePatch.Initialize(log, archipelago);
            QuitToMainMenuPatch.Initialize(log);
            SaveGameDataPatch.Initialize(log);
            StartNewGamePatch.Initialize(log);
        }

        private static void InitializeAwardmentPatches(ManualLogSource log, LocationChecker locationChecker)
        {
            IGetThatReferencePatch.Initialize(log, locationChecker);
            NiceTryPatch.Initialize(log, locationChecker);
            NotExactlyNoblePatch.Initialize(log, locationChecker);
            StoryIsImportantPatch.Initialize(log, locationChecker);
        }

        private static void InitializeAntiCrashPatches(ManualLogSource log)
        {
            var nodeCleaner = new NodeCleaner(log);
            AudioManagerPausePatch.Initialize(log);
            AudioManagerResumePatch.Initialize(log);
            PhysicsManagerPerformStepPatch.Initialize(log, nodeCleaner);
            PlayerUpdateAnimationsPatch.Initialize(log);
            DrawScenePatch.Initialize(log);
            UpdateScenePatch.Initialize(log, nodeCleaner);
            ScreenManagerDrawPatch.Initialize(log);
        }
    }
}
