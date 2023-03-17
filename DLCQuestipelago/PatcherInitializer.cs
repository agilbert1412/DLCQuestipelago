using BepInEx.Logging;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.DLCUnlockPatch;
using DLCQuestipelago.Goal.DLCQuest;
using DLCQuestipelago.Goal.LFOD;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
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
            GetPickaxePatch.Initialize(log, locationChecker);
            GrooveGiveMattockPatch.Initialize(log, locationChecker);
            StoreScreenSetupEntriesPatch.Initialize(log, locationChecker, itemManager.ItemParser);
            TriggerUtilBossDoorPatch.Initialize(log, archipelago);
            InitializeDLCUnlockPatches(log);
            InitializeAllObjectivePatches(log, objectivePersistence);
        }

        private static void InitializeDLCUnlockPatches(ManualLogSource log)
        {
            GrindstoneUnlockPackPatch.Initialize(log);
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
    }
}
