using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLCQuestipelago.Archipelago;

namespace DLCQuestipelago.Serialization
{
    public class ObjectivePersistence
    {
        public const string OBJECTIVE_DLCQUEST_FAKE_ENDING = "dlc_fake_ending";
        public const string OBJECTIVE_DLCQUEST_BAD_ENDING = "dlc_bad_ending";
        public const string OBJECTIVE_DLCQUEST_GOOD_ENDING = "dlc_good_ending";
        public const string OBJECTIVE_DLCQUEST_100_PERCENT = "dlc_100_percent";

        public const string OBJECTIVE_LFOD_FAKE_ENDING = "lfod_fake_ending";
        public const string OBJECTIVE_LFOD_TRUE_ENDING = "lfod_true_ending";
        public const string OBJECTIVE_LFOD_100_PERCENT = "lfod_100_percent";

        private const string TRUE = "true";

        private ArchipelagoClient _archipelago;


        public ObjectivePersistence(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
        }

        public void CheckGoalCompletion()
        {
            if (_archipelago.SlotData.Campaign != Campaign.LiveFreemiumOrDie)
            {
                if (_archipelago.SlotData.Ending == Ending.Any)
                {
                    if (!HasCompletedObjective(OBJECTIVE_DLCQUEST_BAD_ENDING) &&
                        !HasCompletedObjective(OBJECTIVE_DLCQUEST_GOOD_ENDING))
                    {
                        return;
                    }
                }
                else if (_archipelago.SlotData.Ending == Ending.TrueEnding)
                {
                    if (!HasCompletedObjective(OBJECTIVE_DLCQUEST_GOOD_ENDING))
                    {
                        return;
                    }
                }
            }

            if (_archipelago.SlotData.Campaign != Campaign.Basic)
            {
                if (!HasCompletedObjective(OBJECTIVE_LFOD_TRUE_ENDING))
                {
                    return;
                }
            }
            
            _archipelago.ReportGoalCompletion();
            return;
        }

        public void CompleteDlcBadEnding()
        {
            RegisterObjectiveCompletion(OBJECTIVE_DLCQUEST_BAD_ENDING);
        }

        public void CompleteDlcFakeEnding()
        {
            RegisterObjectiveCompletion(OBJECTIVE_DLCQUEST_FAKE_ENDING);
        }

        public void CompleteDlcGoodEnding()
        {
            RegisterObjectiveCompletion(OBJECTIVE_DLCQUEST_GOOD_ENDING);
        }

        public void CompleteLfodFakeEnding()
        {
            RegisterObjectiveCompletion(OBJECTIVE_LFOD_FAKE_ENDING);
        }

        public void CompleteLfodTrueEnding()
        {
            RegisterObjectiveCompletion(OBJECTIVE_LFOD_TRUE_ENDING);
        }

        private void RegisterObjectiveCompletion(string key)
        {
            var keyOnMySlot = GetKeyOnMySlot(key);
            _archipelago.SetStringDataStorage(keyOnMySlot, TRUE);
            CheckGoalCompletion();
        }

        private bool HasCompletedObjective(string key)
        {
            var keyOnMySlot = GetKeyOnMySlot(key);
            var completionValue = _archipelago.ReadStringFromDataStorage(keyOnMySlot);
            if (string.IsNullOrWhiteSpace(completionValue))
            {
                return false;
            }

            return completionValue.ToLower() == TRUE;
        }

        private string GetKeyOnMySlot(string key)
        {
            return $"{_archipelago.SlotData.SlotName}_{key}";
        }
    }
}
