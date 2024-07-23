using System;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.Extensions
{
    public static class TaskExtensions
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        public static async void FireAndForget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred in FireAndForget task: {ex}");
            }
        }
    }
}
