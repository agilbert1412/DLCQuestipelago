using BepInEx.Logging;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Archipelago.Deathlink;
using DLCQuestipelago.DLCUnlockPatch;
using DLCQuestipelago.Items;
using DLCQuestipelago.ItemShufflePatches;
using DLCQuestipelago.Locations;
using DLCQuestipelago.PlayerName;
using DLCQuestipelago.Serialization;

namespace DLCQuestipelago
{
    public static class PatcherInitializer
    {
        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker, ItemManager itemManager, ObjectivePersistence objectivePersistence)
        {
            FrameUpdatePatch.Initialize(log);
            StatsScreenInvalidateAttemptPatch.Initialize(log, locationChecker, itemManager.ItemParser);
            DLCPackPurchasePatch.Initialize(log, locationChecker);
            DLCIsPurchasedPatch.Initialize(log, itemManager.ItemParser);
            CoinPickupPatch.Initialize(log, archipelago, locationChecker);
            InventoryCoinsGetPatch.Initialize(log, archipelago);
            GrooveGiveMattockPatch.Initialize(log, locationChecker);
            StoreScreenSetupEntriesPatch.Initialize(log, locationChecker, itemManager.ItemParser);
            TriggerUtilBossDoorPatch.Initialize(log, archipelago);
            DiePatch.Initialize(log, archipelago);
            InitializeItemShufflePatches(log, archipelago, locationChecker);
            InitializeDLCUnlockPatches(log, locationChecker);
            InitializeAllObjectivePatches(log, objectivePersistence);
            InitializeNameChangePatches(log, archipelago);
        }

        private static void InitializeItemShufflePatches(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            CompleteFetchQuestPatch.Initialize(log, archipelago, locationChecker);
            FetchNpcActivatePatch.Initialize(log, archipelago, locationChecker);
            GetPickaxePatch.Initialize(log, archipelago, locationChecker);
            GrantGunPatch.Initialize(log, archipelago, locationChecker);
            GrantSwordPatch.Initialize(log, archipelago, locationChecker);
            GrantWoodenSwordPatch.Initialize(log, archipelago, locationChecker);
            PickupBoxOfSuppliesPatch.Initialize(log, archipelago, locationChecker);
            RockDestructionPatch.Initialize(log);
        }

        private static void InitializeDLCUnlockPatches(ManualLogSource log, LocationChecker locationChecker)
        {
            GrindstoneUnlockPackPatch.Initialize(log, locationChecker);
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

        private static void InitializeNameChangePatches(ManualLogSource log, ArchipelagoClient archipelago)
        {
            PlayerToSlotNamePatch.Initialize(log, archipelago);
            PurchaseNameChangePatch.Initialize(log, archipelago);
        }
    }
}
