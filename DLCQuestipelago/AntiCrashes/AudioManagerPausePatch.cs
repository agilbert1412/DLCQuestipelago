using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Core.Audio;
using HarmonyLib;

namespace DLCQuestipelago.AntiCrashes
{
    [HarmonyPatch(typeof(XACTAudioSystem))]
    [HarmonyPatch(nameof(XACTAudioSystem.PauseMusic))]
    public static class AudioManagerPausePatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public void PauseMusic()
        private static bool Prefix(XACTAudioSystem __instance)
        {
            try
            {
                if (__instance.CurrentSongCue == null)
                {
                    return false; // don't run original logic
                }

                __instance.CurrentSongCue.Pause();
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AudioManagerPausePatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return false; // don't run original logic
            }
        }
    }
}
