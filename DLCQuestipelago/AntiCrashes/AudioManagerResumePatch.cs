using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Core.Audio;
using HarmonyLib;

namespace DLCQuestipelago.AntiCrashes
{
    [HarmonyPatch(typeof(XACTAudioSystem))]
    [HarmonyPatch(nameof(XACTAudioSystem.ResumeMusic))]
    public static class AudioManagerResumePatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // public void ResumeMusic()
        private static bool Prefix(XACTAudioSystem __instance)
        {
            try
            {
                if (__instance.CurrentSongCue == null)
                {
                    return false; // don't run original logic
                }

                __instance.CurrentSongCue.Resume();
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AudioManagerResumePatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return false; // don't run original logic
            }
        }
    }
}
