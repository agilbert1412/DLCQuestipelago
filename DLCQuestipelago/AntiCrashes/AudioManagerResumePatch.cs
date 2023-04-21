using System;
using System.Diagnostics;
using BepInEx.Logging;
using Core.Audio;
using HarmonyLib;

namespace DLCQuestipelago.AntiCrashes
{
    [HarmonyPatch(typeof(XACTAudioSystem))]
    [HarmonyPatch(nameof(XACTAudioSystem.ResumeMusic))]
    public static class AudioManagerResumePatch
    {
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log)
        {
            _log = log;
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
                _log.LogError($"Failed in {nameof(AudioManagerResumePatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return false; // don't run original logic
            }
        }
    }
}
