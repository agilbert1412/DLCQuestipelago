using BepInEx.Logging;
using Core;
using DLCLib;
using DLCLib.Character;
using DLCLib.Conversation;
using DLCLib.DLC;
using DLCLib.Scripts.LFOD;
using DLCLib.World.Props;
using DLCQuestipelago.Archipelago;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.ItemShufflePatches
{
    [HarmonyPatch(typeof(MiddleAgedManNPC))]
    [HarmonyPatch(nameof(MiddleAgedManNPC.Activate))]
    public static class MiddleAgeNpcActivatePatch
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

        //public override bool Activate()
        private static void Postfix(FetchNPC __instance, ref bool __result)
        {
            if (_archipelago.SlotData.ItemShuffle == ItemShuffle.Disabled)
            {
                return;
            }

            var hasCheckedSwordLocation = _locationChecker.IsLocationChecked("Wooden Sword");

            if (!Singleton<DLCManager>.Instance.IsUnlocked("coloredtext"))
            {
                __instance.SetCurrentConversation("spawncoloredtextpack");
            }
            else if (!Singleton<DLCManager>.Instance.IsPurchased("coloredtext", false))
            {
                __instance.SetCurrentConversation("buycoloredtextpack");
            }
            else if (!hasCheckedSwordLocation)
            {
                __instance.SetCurrentConversation("warnshepherd");
            }
            else if (!Singleton<SceneManager>.Instance.CurrentScene.EventList.Contains(ShepherdEncounter.SHEPHERD_ENCOUNTER_COMPLETE_STR))
            {
                __instance.SetCurrentConversation("findandkillshepherd");
            }
            else
            {
                __instance.SetCurrentConversation("aftershpeherddead");
            }

            return;
        }
    }
}
