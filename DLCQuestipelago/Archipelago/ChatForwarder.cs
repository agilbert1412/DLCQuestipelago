namespace DLCQuestipelago.Archipelago
{
    public class ChatForwarder
    {
        public const string COMMAND_PREFIX = "!!";

        private static Logger _console;
        private static ArchipelagoClient _archipelago;
        private HarmonyLib.Harmony _harmony;

        public ChatForwarder(Logger console, HarmonyLib.Harmony harmony)
        {
            _console = console;
            _harmony = harmony;
        }

        public void ListenToChatMessages(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
            // TODO
        }

        public static void ReceiveChatMessage_ForwardToAp_PostFix()
        {
            // TODO
        }

        private static bool TryHandleCommand(string message)
        {
            if (message == null || !message.StartsWith(COMMAND_PREFIX))
            {
                return false;
            }

            var messageLower = message.ToLower();
            if (HandleGoalCommand(messageLower))
            {
                return true;
            }

            if (HandleSyncCommand(messageLower))
            {
                return true;
            }

            if (HandleHelpCommand(messageLower))
            {
                return true;
            }

            if (message.StartsWith(COMMAND_PREFIX))
            {
                _console.LogInfo($"Unrecognized command. Use {COMMAND_PREFIX}help for a list of commands");
                return true;
            }

            return false;
        }

        private static bool HandleGoalCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}goal")
            {
                return false;
            }

            /*var goal = GoalCodeInjection.GetGoalString();
            var goalMessage = $"Your Goal is: {goal}";
            Game1.chatBox?.addMessage(goalMessage, Color.Gold);*/
            return true;
        }

        private static bool HandleSyncCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}sync")
            {
                return false;
            }

            _archipelago.Sync();
            return true;
        }

        private static bool HandleHelpCommand(string message)
        {
            if (message != $"{COMMAND_PREFIX}help")
            {
                return false;
            }

            PrintCommandHelp();
            return true;
        }

        private static void PrintCommandHelp()
        {
            _console.LogInfo($"{COMMAND_PREFIX}help - Shows the list of client commands");
            _console.LogInfo($"{COMMAND_PREFIX}goal - Shows your current Archipelago Goal");
#if DEBUG
            _console.LogInfo($"{COMMAND_PREFIX}sync - Sends a Sync packet to the Archipelago server");
#endif
        }
    }
}
