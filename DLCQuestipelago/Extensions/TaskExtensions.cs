﻿using System;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace DLCQuestipelago.Extensions
{
    public static class TaskExtensions
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
        }

        public static async void FireAndForget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _log.LogError($"Exception occurred in FireAndForget task: {ex}");
            }
        }
    }
}
