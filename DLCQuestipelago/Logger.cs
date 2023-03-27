using MelonLoader;

namespace DLCQuestipelago
{
    public class Logger
    {
        private MelonLogger.Instance _internalLogger;

        public Logger(MelonLogger.Instance internalLogger)
        {
            _internalLogger = internalLogger;
        }

        public void LogDebug(string text)
        {
            _internalLogger.Msg(text);
        }

        public void LogInfo(string text)
        {
            _internalLogger.Msg(text);
        }

        public void LogWarning(string text)
        {
            _internalLogger.Warning(text);
        }

        public void LogError(string text)
        {
            _internalLogger.Error(text);
        }
    }
}
