﻿using System;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using DLCLib.DLC;
using DLCQuestipelago.Items;
using DLCQuestipelago.Locations;
using HarmonyLib;

namespace DLCQuestipelago.Shop
{
    [HarmonyPatch(typeof(DLCPack))]
    [HarmonyPatch("Purchase")]
    public static class DLCPackPurchasePatch
    {
        private static ManualLogSource _log;
        private static LocationChecker _locationChecker;

        public static void Initialize(ManualLogSource log, LocationChecker locationChecker)
        {
            _log = log;
            _locationChecker = locationChecker;
        }

        private static bool Prefix(DLCPack __instance)
        {
            try
            {
                if (__instance == null)
                {
                    return false; // don't run original logic
                }

                var stateProperty = typeof(DLCPack).GetProperty("State");
                stateProperty.SetValue(__instance, DLCPackStateEnum.Purchased);

                if (_locationChecker == null)
                {
                    return false; // don't run original logic
                }

                _log?.LogInfo($"Purchased a DLC! [{__instance.Data.DisplayName}]");
                _locationChecker.AddCheckedLocation(__instance.Data.DisplayName);

                if (__instance.Data.IsBossDLC)
                {
                    if (__instance.Data.PurchaseEvent != null && !__instance.Data.PurchaseEvent.Equals(string.Empty))
                    {
                        typeof(DLCPurchaseEventUtil).InvokeMember(__instance.Data.PurchaseEvent,
                            BindingFlags.InvokeMethod,
                            null, null, new object[0]);
                    }
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed in {nameof(DLCPackPurchasePatch)}.{nameof(Prefix)}:\n\t{ex}");
                Debugger.Break();
                return true; // run original logic
            }
        }

        private static void Postfix(DLCPack __instance)
        {
            CoinsanityUtils.UpdateCoinsUI();
        }
    }
}
