using System;
using System.Collections.Generic;
using BepInEx.Logging;
using DLCLib.World.Props;

namespace DLCQuestipelago.Archipelago
{
    public class SlotData
    {
        private const string COINSANITY_KEY = "coinsanity";
        private const string ENDING_KEY = "ending_choice"; // any0 true1
        private const string CAMPAIGN_KEY = "campaign"; // basic0 live_freemium_or_die1 both2
        private const string DEATH_LINK_KEY = "death_link";
        private const string SEED_KEY = "seed";
        private const string MULTIWORLD_VERSION_KEY = "client_version";

        private Dictionary<string, object> _slotDataFields;
        private ManualLogSource _console;

        public string SlotName { get; private set; }
        public Coinsanity Coinsanity { get; private set; }
        public Ending Ending { get; private set; }
        public Campaign Campaign { get; private set; }
        public bool DeathLink { get; private set; }
        public string Seed { get; private set; }
        public string MultiworldVersion { get; private set; }

        public SlotData(string slotName, Dictionary<string, object> slotDataFields, ManualLogSource console)
        {
            SlotName = slotName;
            _slotDataFields = slotDataFields;
            _console = console;

            Coinsanity = GetSlotSetting(COINSANITY_KEY, Coinsanity.Disabled);
            Ending = GetSlotSetting(ENDING_KEY, Ending.TrueEnding);
            Campaign = GetSlotSetting(CAMPAIGN_KEY, Campaign.LiveFreemiumOrDie);
            DeathLink = GetSlotSetting(DEATH_LINK_KEY, false);
            Seed = GetSlotSetting(SEED_KEY, "");
            MultiworldVersion = GetSlotSetting(MULTIWORLD_VERSION_KEY, "");
        }

        private T GetSlotSetting<T>(string key, T defaultValue) where T : struct, Enum, IConvertible
        {
            return _slotDataFields.ContainsKey(key) ? (T)Enum.Parse(typeof(T), _slotDataFields[key].ToString(), true) : GetSlotDefaultValue(key, defaultValue);
        }

        private string GetSlotSetting(string key, string defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? _slotDataFields[key].ToString() : GetSlotDefaultValue(key, defaultValue);
        }

        private int GetSlotSetting(string key, int defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? (int)(long)_slotDataFields[key] : GetSlotDefaultValue(key, defaultValue);
        }

        private bool GetSlotSetting(string key, bool defaultValue)
        {
            if (_slotDataFields.ContainsKey(key) && _slotDataFields[key] != null)
            {
                if (_slotDataFields[key] is bool boolValue)
                {
                    return boolValue;
                }

                if (_slotDataFields[key] is long longValue)
                {
                    return longValue != 0;
                }

                if (_slotDataFields[key] is int intValue)
                {
                    return intValue != 0;
                }
            }

            return GetSlotDefaultValue(key, defaultValue);
        }

        private T GetSlotDefaultValue<T>(string key, T defaultValue)
        {
            _console.LogWarning($"SlotData did not contain expected key: \"{key}\"");
            return defaultValue;
        }
    }

    public enum Coinsanity
    {
        Disabled = 0,
    }

    public enum Ending
    {
        Any = 0,
        TrueEnding = 1,
    }

    public enum Campaign
    {
        Basic = 0,
        LiveFreemiumOrDie = 1,
        Both = 2,
    }
}
