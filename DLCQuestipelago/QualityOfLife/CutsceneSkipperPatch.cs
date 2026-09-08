using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using DLCLib;
using DLCLib.Conversation;
using DLCLib.Input;
using DLCLib.NIS;
using DLCLib.Screens;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework.Input;

namespace DLCQuestipelago.QualityOfLife
{
    [HarmonyPatch(typeof(NISManager))]
    [HarmonyPatch("Update")]
    public static class CutsceneSkipperPatch
    {
        public const string DEFAULT_CUTSCENE_SKIP_KEY = "C";

        private static ILogger _logger;
        private static KeyboardState _previousKeyboardState;
        private static bool _announcedConfiguration;
        private static NISScript _lastSeenNIS;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public void Update(float dt)
        private static void Postfix(NISManager __instance)
        {
            try
            {
                HandleSkipInput(__instance);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed in {nameof(CutsceneSkipperPatch)}.{nameof(Postfix)}:\n\t{ex}");
                Debugger.Break();
                return;
            }
        }

        private static void HandleSkipInput(NISManager nisManager)
        {
            var inputState = DLCInputManager.Instance.InputState;
            if (inputState == null)
            {
                return;
            }

            var currentKeyboardState = inputState.CurrentKeyboardState;
            var skipKeys = GetConfiguredSkipKeys();
            if (!_announcedConfiguration)
            {
                _announcedConfiguration = true;
                LogInfo($"[CutsceneSkipper] Patch is running. Configured skip keys: [{string.Join(", ", skipKeys)}]");
            }

            var currentNIS = GetCurrentNIS(nisManager);
            if (currentNIS != null && !ReferenceEquals(currentNIS, _lastSeenNIS))
            {
                _lastSeenNIS = currentNIS;
                LogInfo($"[CutsceneSkipper] NIS detected: {GetScriptName(currentNIS)}");
            }

            var skipKeyWasDownLastFrame = false;
            var skipKeyIsDownThisFrame = false;
            foreach (var skipKey in skipKeys)
            {
                skipKeyWasDownLastFrame |= _previousKeyboardState.IsKeyDown(skipKey);
                skipKeyIsDownThisFrame |= currentKeyboardState.IsKeyDown(skipKey);
            }
            _previousKeyboardState = currentKeyboardState;

            if (!skipKeyIsDownThisFrame || skipKeyWasDownLastFrame)
            {
                return;
            }

            if (currentNIS == null)
            {
                return;
            }

            LogInfo($"[CutsceneSkipper] Skipping NIS {GetScriptName(currentNIS)}");
            CloseCurrentConversation();
            EndCurrentNIS(currentNIS);
            CloseCurrentConversation();
        }

        private static void LogInfo(string message)
        {
            _logger?.LogInfo(message);
        }

        private static List<Keys> GetConfiguredSkipKeys()
        {
            var skipKeys = new List<Keys>();
            var skipKeysString = Plugin.Instance.APConnectionInfo.CutsceneSkipKey;
            if (string.IsNullOrWhiteSpace(skipKeysString))
            {
                return skipKeys;
            }

            foreach (var skipKeyChar in skipKeysString)
            {
                var skipKeyString = skipKeyChar.ToString();
                if (string.IsNullOrWhiteSpace(skipKeyString))
                {
                    continue;
                }
                if (Enum.TryParse<Keys>(skipKeyString, true, out var key))
                {
                    skipKeys.Add(key);
                }
            }

            return skipKeys;
        }

        private static NISScript GetCurrentNIS(NISManager nisManager)
        {
            var currentNISField = typeof(NISManager).GetField("currentNIS", BindingFlags.NonPublic | BindingFlags.Instance);
            return currentNISField?.GetValue(nisManager) as NISScript;
        }

        private static string GetScriptName(NISScript nisScript)
        {
            var dataField = typeof(NISScript).GetField("data", BindingFlags.NonPublic | BindingFlags.Instance);
            var data = dataField?.GetValue(nisScript);
            if (data == null)
            {
                return "<unknown>";
            }

            var dataType = data.GetType();
            var nameProperty = dataType.GetProperty("Name");
            if (nameProperty != null)
            {
                return nameProperty.GetValue(data) as string ?? "<unknown>";
            }

            var nameField = dataType.GetField("Name");
            return nameField?.GetValue(data) as string ?? "<unknown>";
        }

        private static void CloseCurrentConversation()
        {
            var conversationManager = SceneManager.Instance?.CurrentScene?.ConversationManager;
            if (conversationManager == null || !conversationManager.IsActive)
            {
                return;
            }

            var dialogPopupField = typeof(ConversationManager).GetField("dialogPopup", BindingFlags.NonPublic | BindingFlags.Instance);
            var dialogPopup = dialogPopupField?.GetValue(conversationManager) as DialogPopup;
            if (dialogPopup == null)
            {
                return;
            }

            dialogPopup.EndConversation();
            ResetNISConversationFlags();
        }

        private static void ResetNISConversationFlags()
        {
            var startedField = typeof(NISScript).GetField("ConversationStarted", BindingFlags.NonPublic | BindingFlags.Static);
            startedField?.SetValue(null, false);
            var completeField = typeof(NISScript).GetField("ConversationComplete", BindingFlags.NonPublic | BindingFlags.Static);
            completeField?.SetValue(null, false);
        }

        private static void EndCurrentNIS(NISScript currentNIS)
        {
            var endScriptMethod = typeof(NISScript).GetMethod("EndScript", BindingFlags.NonPublic | BindingFlags.Instance);
            endScriptMethod?.Invoke(currentNIS, null);
        }
    }
}
