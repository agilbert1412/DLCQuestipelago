using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
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
using DLCQuestipelago.MoveLink;
using KaitoKid.ArchipelagoUtilities.Net;
using DLCQuestipelago.PlayerName;
using DLCQuestipelago.Serialization;
using DLCQuestipelago.Shop;
using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace DLCQuestipelago
{
    public static class PatcherInitializer
    {
        public static void InitializeEarly(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            InitializeAllPersistencyPatches(logger, archipelago);
        }

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago, LocationChecker locationChecker, DLCQItemManager itemManager, ObjectivePersistence objectivePersistence, GiftSender giftSender)
        {
            InitializeDualContentManagerPatches(logger);
            FrameUpdatePatch.Initialize(logger);
            StatsScreenInvalidateAttemptPatch.Initialize(logger);
            DLCPackPurchasePatch.Initialize(logger, locationChecker);
            DLCIsPurchasedPatch.Initialize(logger, itemManager.ItemParser);
            InventoryMakePurchasePatch.Initialize(logger, archipelago);
            StoreScreenSelectionChangedPatch.Initialize(logger, archipelago);
            CoinPickupPatch.Initialize(logger, archipelago, locationChecker);
            InventoryCoinsGetPatch.Initialize(logger, archipelago);
            HandleCoinChangedPatch.Initialize(logger, archipelago);
            GrooveGiveMattockPatch.Initialize(logger, locationChecker);
            StoreScreenSetupEntriesPatch.Initialize(logger, locationChecker, itemManager.ItemParser);
            TriggerUtilBossDoorPatch.Initialize(logger, archipelago);
            BossSheepAttackPatch.Initialize(logger, archipelago);
            DiePatch.Initialize(logger, archipelago);
            InitializeItemShufflePatches(logger, archipelago, locationChecker);
            InitializeDLCUnlockPatches(logger, archipelago, locationChecker);
            InitializeAllObjectivePatches(logger, objectivePersistence);
            InitializeFakeEndingDisconnectPatches(logger, archipelago);
            InitializeNameChangePatches(logger, archipelago);
            InitializeKillAndDestroyPatches(logger, locationChecker, giftSender);
            InitializeAwardmentPatches(logger, locationChecker);
            InitializeAntiCrashPatches(logger);
            CoinsAppearancePatch.ReplaceBrokenCoins(logger, archipelago);
            MoveLinkManager.Initialize(logger, archipelago);
        }

        private static void InitializeKillAndDestroyPatches(ILogger logger, LocationChecker locationChecker,
            GiftSender giftSender)
        {
            DestroyBushPatch.Initialize(logger, giftSender);
            DestroyRockPatch.Initialize(logger, giftSender);
            DestroyVinePatch.Initialize(logger, giftSender);
            KillSheepPatch.Initialize(logger, locationChecker, giftSender);
        }

        private static void InitializeDualContentManagerPatches(ILogger logger)
        {
            ConstructAnimationPatch.Initialize(logger);
        }

        private static void InitializeItemShufflePatches(ILogger logger, DLCQArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            CompleteFetchQuestPatch.Initialize(logger, archipelago, locationChecker);
            GetPickaxePatch.Initialize(logger, archipelago, locationChecker);
            GrantGunPatch.Initialize(logger, archipelago, locationChecker);
            GrantSwordPatch.Initialize(logger, archipelago, locationChecker);
            GrantWoodenSwordPatch.Initialize(logger, archipelago, locationChecker);
            PickupBoxOfSuppliesPatch.Initialize(logger, archipelago, locationChecker);
            RockDestructionPatch.Initialize(logger);
            var conversationStarter = new ConversationStarter();
            MiddleAgeNpcActivatePatch.Initialize(logger, archipelago, locationChecker, conversationStarter);
            GrooveNpcActivatePatch.Initialize(logger, archipelago, locationChecker, conversationStarter);
            FetchNpcActivatePatch.Initialize(logger, archipelago, locationChecker, conversationStarter);
        }

        private static void InitializeDLCUnlockPatches(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            GrindstoneUnlockPackPatch.Initialize(logger, archipelago, locationChecker);
            RandomEncounterUnlockPackPatch.Initialize(logger);
            TriggerUtilActivateForestPatch.Initialize(logger);
            TriggerUtilActivateNightForestPatch.Initialize(logger);
            TriggerUtilGameEndingPatch.Initialize(logger);
            ZoneOnEnterPatch.Initialize(logger);
            ExamineBanWallPatch.Initialize(logger);
            ComedianDLCPatch.Initialize(logger);
        }

        private static void InitializeAllObjectivePatches(ILogger logger, ObjectivePersistence objectivePersistence)
        {
            InitializeDlcQuestObjectivePatches(logger, objectivePersistence);
            InitializeLfodObjectivePatches(logger, objectivePersistence);
        }

        private static void InitializeDlcQuestObjectivePatches(ILogger logger, ObjectivePersistence objectivePersistence)
        {
            Goal.DLCQuest.FakeEndingObjectivePatch.Initialize(logger, objectivePersistence);
            Goal.DLCQuest.BadEndingObjectivePatch.Initialize(logger, objectivePersistence);
            Goal.DLCQuest.GoodEndingObjectivePatch.Initialize(logger, objectivePersistence);
        }

        private static void InitializeLfodObjectivePatches(ILogger logger, ObjectivePersistence objectivePersistence)
        {
            Goal.LFOD.FakeEndingObjectivePatch.Initialize(logger, objectivePersistence);
            Goal.LFOD.TrueEndingObjectivePatch.Initialize(logger, objectivePersistence);
        }

        private static void InitializeFakeEndingDisconnectPatches(ILogger logger, ArchipelagoClient archipelago)
        {
            FakeEndingDisconnectPatch.Initialize(logger, archipelago);
            FailedReconnectPatch.Initialize(logger, archipelago);
            SuccessReconnectPatch.Initialize(logger, archipelago);
        }

        private static void InitializeNameChangePatches(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            var nameChanger = new NameChanger(archipelago);
            PlayerNameInDialogPatch.Initialize(logger, archipelago, nameChanger);
            PlayerNameInMessageBoxPatch.Initialize(logger, archipelago, nameChanger);
            PurchaseNameChangePatch.Initialize(logger, archipelago, nameChanger);
        }

        private static void InitializeAllPersistencyPatches(ILogger logger, DLCQArchipelagoClient archipelago)
        {
            ClearSaveGamePatch.Initialize(logger);
            ContinueGamePatch.Initialize(logger);
            GetSaveFilenamePatch.Initialize(logger, archipelago);
            QuitToMainMenuPatch.Initialize(logger);
            SaveGameDataPatch.Initialize(logger);
            StartNewGamePatch.Initialize(logger);
        }

        private static void InitializeAwardmentPatches(ILogger logger, LocationChecker locationChecker)
        {
            IGetThatReferencePatch.Initialize(logger, locationChecker);
            NiceTryPatch.Initialize(logger, locationChecker);
            NotExactlyNoblePatch.Initialize(logger, locationChecker);
            StoryIsImportantPatch.Initialize(logger, locationChecker);
        }

        private static void InitializeAntiCrashPatches(ILogger logger)
        {
            var nodeCleaner = new NodeCleaner(logger);
            AudioManagerPausePatch.Initialize(logger);
            AudioManagerResumePatch.Initialize(logger);
            PhysicsManagerPerformStepPatch.Initialize(logger, nodeCleaner);
            PlayerUpdateAnimationsPatch.Initialize(logger);
            DrawScenePatch.Initialize(logger);
            UpdateScenePatch.Initialize(logger, nodeCleaner);
            ScreenManagerDrawPatch.Initialize(logger);
        }
    }
}
