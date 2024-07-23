using BepInEx.Logging;
using Logger = KaitoKid.ArchipelagoUtilities.Net.Client.Logger;

namespace DLCQuestipelago.Utilities
{
    public class LogHandler : Logger
    {
        private readonly ManualLogSource _logger;

        public LogHandler(ManualLogSource logger)
        {
            _logger = logger;
        }

        public override void LogError(string message)
        {
            _logger.LogError(message);
        }

        public override void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public override void LogMessage(string message)
        {
            _logger.LogMessage(message);
        }

        public override void LogInfo(string message)
        {
            _logger.LogInfo(message);
        }

        public override void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }
    }
}
