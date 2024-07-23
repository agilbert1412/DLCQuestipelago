using System;
using System.Diagnostics;
using Core;
using DLCLib;
using DLCLib.Character;
using DLCLib.DLC;
using DLCLib.Scripts.LFOD;
using DLCQuestipelago.Archipelago;
using KaitoKid.ArchipelagoUtilities.Net;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(MiddleAgedManNPC))]
    [HarmonyPatch(nameof(MiddleAgedManNPC.Activate))]
    public static class MiddleAgeNpcActivatePatch
    {
        private static ILogger _logger;
        private static DLCQArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ConversationStarter _conversationStarter;

        public static void Initialize(ILogger logger, DLCQArchipelagoClient archipelago, LocationChecker locationChecker, ConversationStarter conversationStarter)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _conversationStarter = conversationStarter;
        }

        //public override bool Activate()
        private static bool Prefix(FetchNPC __instance, ref bool __result)
        {
            try
            {
                if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
                {
                    return true;
                }

                var hasCheckedSwordLocation = _locationChecker.IsLocationChecked("Wooden Sword");

                if (!Singleton<DLCManager>.Instance.IsUnlocked("coloredtext"))
                {
                    __result = _conversationStarter.StartConversation(__instance, "spawncoloredtextpack");
                }
                else if (!Singleton<DLCManager>.Instance.IsPurchased("coloredtext", false))
                {
                    __result = _conversationStarter.StartConversation(__instance, "buycoloredtextpack");
                }
                else if (!hasCheckedSwordLocation)
                {
                    __result = _conversationStarter.StartConversation(__instance, "warnshepherd");
                }
                else if (!Singleton<SceneManager>.Instance.CurrentScene.EventList.Contains(ShepherdEncounter
                             .SHEPHERD_ENCOUNTER_COMPLETE_STR))
                {
                    __result = _conversationStarter.StartConversation(__instance, "findandkillshepherd");
                }
                else
                {
                    __result = _conversationStarter.StartConversation(__instance, "aftershpeherddead");
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MiddleAgeNpcActivatePatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }
    }
}
