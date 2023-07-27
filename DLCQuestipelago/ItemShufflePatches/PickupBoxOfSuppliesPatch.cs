using System;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using DLCLib;
using DLCLib.Audio;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(FetchQuestPickup))]
    [HarmonyPatch(nameof(FetchQuestPickup.OnPickup))]
    public static class PickupBoxOfSuppliesPatch
    {
        private static ManualLogSource _log;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ManualLogSource log, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _log = log;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        //public override void OnPickup(Player player)
        private static bool Prefix(FetchQuestPickup __instance, Player player)
        {
            try
            {
                if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
                {
                    return true; // run original logic
                }

                _locationChecker.AddCheckedLocation("Box of Various Supplies");
                var scene = SceneManager.Instance.CurrentScene;
                DLCAudioManager.Instance.PlaySound("pickup_dlc");
                scene.RemoveFromScene(__instance);
                var sceneType = typeof(Scene);
                var recordPickupMethod = sceneType.GetMethod("RecordPickup", BindingFlags.NonPublic | BindingFlags.Instance);
                recordPickupMethod.Invoke(scene, new object[] { __instance });
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(PickupBoxOfSuppliesPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
