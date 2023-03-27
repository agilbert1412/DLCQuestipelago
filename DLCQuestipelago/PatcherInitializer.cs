﻿using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Archipelago.Deathlink;
using DLCQuestipelago.DLCUnlockPatch;
using DLCQuestipelago.DualContentManager;
using DLCQuestipelago.FakeEndingBehavior;
using DLCQuestipelago.Items;
using DLCQuestipelago.Items.Traps;
using DLCQuestipelago.ItemShufflePatches;
using DLCQuestipelago.Locations;
using DLCQuestipelago.PlayerName;
using DLCQuestipelago.Serialization;

namespace DLCQuestipelago
{
    public static class PatcherInitializer
    {
        public static void Initialize(Logger log, ArchipelagoClient archipelago, LocationChecker locationChecker, ItemManager itemManager, ObjectivePersistence objectivePersistence)
        {
            InitializeDualContentManagerPatches(log);
            FrameUpdatePatch.Initialize(log);
            StatsScreenInvalidateAttemptPatch.Initialize(log, locationChecker, itemManager.ItemParser);
            DLCPackPurchasePatch.Initialize(log, locationChecker);
            DLCIsPurchasedPatch.Initialize(log, itemManager.ItemParser);
            CoinPickupPatch.Initialize(log, archipelago, locationChecker);
            InventoryCoinsGetPatch.Initialize(log, archipelago);
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
            KillSheepPatch.Initialize(log, locationChecker);
            InitializeAwardmentPatches(log, locationChecker);
        }

        private static void InitializeDualContentManagerPatches(Logger log)
        {
            ConstructAnimationPatch.Initialize(log);
        }

        private static void InitializeItemShufflePatches(Logger log, ArchipelagoClient archipelago, LocationChecker locationChecker)
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

        private static void InitializeDLCUnlockPatches(Logger log, ArchipelagoClient archipelago, LocationChecker locationChecker)
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

        private static void InitializeAllObjectivePatches(Logger log, ObjectivePersistence objectivePersistence)
        {
            InitializeDlcQuestObjectivePatches(log, objectivePersistence);
            InitializeLfodObjectivePatches(log, objectivePersistence);
        }

        private static void InitializeDlcQuestObjectivePatches(Logger log, ObjectivePersistence objectivePersistence)
        {
            Goal.DLCQuest.FakeEndingObjectivePatch.Initialize(log, objectivePersistence);
            Goal.DLCQuest.BadEndingObjectivePatch.Initialize(log, objectivePersistence);
            Goal.DLCQuest.GoodEndingObjectivePatch.Initialize(log, objectivePersistence);
        }

        private static void InitializeLfodObjectivePatches(Logger log, ObjectivePersistence objectivePersistence)
        {
            Goal.LFOD.FakeEndingObjectivePatch.Initialize(log, objectivePersistence);
            Goal.LFOD.TrueEndingObjectivePatch.Initialize(log, objectivePersistence);
        }

        private static void InitializeFakeEndingDisconnectPatches(Logger log, ArchipelagoClient archipelago)
        {
            FakeEndingDisconnectPatch.Initialize(log, archipelago);
            FailedReconnectPatch.Initialize(log, archipelago);
            SuccessReconnectPatch.Initialize(log, archipelago);
        }

        private static void InitializeNameChangePatches(Logger log, ArchipelagoClient archipelago)
        {
            var nameChanger = new NameChanger(archipelago);
            PlayerNameInDialogPatch.Initialize(log, archipelago, nameChanger);
            PlayerNameInMessageBoxPatch.Initialize(log, archipelago, nameChanger);
            PurchaseNameChangePatch.Initialize(log, archipelago, nameChanger);
        }

        private static void InitializeAllPersistencyPatches(Logger log, ArchipelagoClient archipelago)
        {
            ClearSaveGamePatch.Initialize(log);
            ContinueGamePatch.Initialize(log);
            GetSaveFilenamePatch.Initialize(log, archipelago);
            QuitToMainMenuPatch.Initialize(log);
            SaveGameDataPatch.Initialize(log);
            StartNewGamePatch.Initialize(log);
        }

        private static void InitializeAwardmentPatches(Logger log, LocationChecker locationChecker)
        {
            IGetThatReferencePatch.Initialize(log, locationChecker);
            NiceTryPatch.Initialize(log, locationChecker);
            NotExactlyNoblePatch.Initialize(log, locationChecker);
            StoryIsImportantPatch.Initialize(log, locationChecker);
        }
    }
}
