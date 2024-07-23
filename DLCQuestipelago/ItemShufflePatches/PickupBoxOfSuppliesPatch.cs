using System;
using System.Diagnostics;
using System.Reflection;
using DLCLib;
using DLCLib.Audio;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using KaitoKid.ArchipelagoUtilities.Net;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(FetchQuestPickup))]
    [HarmonyPatch(nameof(FetchQuestPickup.OnPickup))]
    public static class PickupBoxOfSuppliesPatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
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
                _logger.LogError($"Failed in {nameof(PickupBoxOfSuppliesPatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
