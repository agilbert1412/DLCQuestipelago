using System.IO;
using System.Reflection;
using DLCLib.Save;
using DLCQuestipelago.Archipelago;
using EasyStorage;
using Newtonsoft.Json;

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
        private const string goalPersistencyFilename = "Goal_{0}.dlc";

        private const string TRUE = "true";

        private ArchipelagoClient _archipelago;


        private static readonly MethodInfo GetSaveFilenameMethod =
            typeof(DLCSaveManager).GetMethod("GetSaveFilename", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo SaveDeviceField =
            typeof(DLCSaveManager).GetField("saveDevice", BindingFlags.NonPublic | BindingFlags.Instance);

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

        private string GetObjectiveFileName(string key)
        {
            var saveFileName = (string)GetSaveFilenameMethod.Invoke(DLCSaveManager.Instance, new object[0]);
            var objective_suffix = string.Format(goalPersistencyFilename, key);
            var objectiveFileName = $"{saveFileName}_{objective_suffix}";

            return objectiveFileName;
        }

        private void RegisterObjectiveCompletion(string key)
        {
            var saveDevice = (SaveDevice)SaveDeviceField.GetValue(DLCSaveManager.Instance);
            saveDevice.Save(DLCSaveManager.Instance.DataSaveDirectory, GetObjectiveFileName(key), (stream) =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write($"{key}: Completed");
                    writer.Flush();
                }
            });
            CheckGoalCompletion();
        }

        private bool HasCompletedObjective(string key)
        {
            var saveDevice = (SaveDevice)SaveDeviceField.GetValue(DLCSaveManager.Instance);
            var saveDirectory = DLCSaveManager.Instance.DataSaveDirectory;
            var persistencyFileName = GetObjectiveFileName(key);
            var fileExists = saveDevice.FileExists(saveDirectory, persistencyFileName);
            if (!fileExists)
            {
                return false;
            }

            var objectivePersistencyContent = "";
            saveDevice.Load(saveDirectory, persistencyFileName, (stream) =>
            {
                using (var reader = new StreamReader(stream))
                {
                    objectivePersistencyContent = reader.ReadToEnd();
                }
            });

            if (string.IsNullOrWhiteSpace(objectivePersistencyContent))
            {
                return false;
            }

            return objectivePersistencyContent == $"{key}: Completed";
        }

        private void A(string key)
        {
        }
    }
}
