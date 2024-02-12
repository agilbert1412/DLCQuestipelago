using BepInEx.Logging;
using System;
using System.Collections.Generic;

namespace DLCQuestipelago.Archipelago
{
    public class SlotData
    {
        private const string COINSANITY_KEY = "coinsanity";
        private static readonly string[] COINSANITY_BUNDLE_KEYS = new[] { "coinbundlerange", "coin_bundle_range",
                                                                          "coinbundlequantity", "coin_bundle_quantity",
                                                                          "coinsanityrange", "coinsanity_range", "coin_sanity_range" };
        private const string PERMANENT_COINS_KEY = "permanent_coins";
        private const string ENDING_KEY = "ending_choice";
        private const string CAMPAIGN_KEY = "campaign";
        private const string ITEM_SHUFFLE_KEY = "item_shuffle";
        private const string DEATH_LINK_KEY = "death_link";
        private const string SEED_KEY = "seed";
        private const string MULTIWORLD_VERSION_KEY = "client_version";

        private Dictionary<string, object> _slotDataFields;
        private ManualLogSource _console;

        public string SlotName { get; private set; }
        public Coinsanity Coinsanity { get; private set; }
        public int CoinBundleSize { get; private set; }

        public double GetRealCoinBundleSize()
        {
            if (Coinsanity == Coinsanity.None)
            {
                return 20;
            }

            return CoinBundleSize <= 0 ? 0.1 : CoinBundleSize;
        }
        public bool PermanentCoins { get; private set; }
        public Ending Ending { get; private set; }
        public Campaign Campaign { get; private set; }
        public ItemShuffle ItemShuffle { get; private set; }
        public bool DeathLink { get; private set; }
        public string Seed { get; private set; }
        public string MultiworldVersion { get; private set; }

        public SlotData(string slotName, Dictionary<string, object> slotDataFields, ManualLogSource console)
        {
            SlotName = slotName;
            _slotDataFields = slotDataFields;
            _console = console;

            Coinsanity = GetSlotSetting(COINSANITY_KEY, Coinsanity.None);
            CoinBundleSize = GetSlotSetting(COINSANITY_BUNDLE_KEYS, 20);
            PermanentCoins = GetSlotSetting(PERMANENT_COINS_KEY, false);
            Ending = GetSlotSetting(ENDING_KEY, Ending.TrueEnding);
            Campaign = GetSlotSetting(CAMPAIGN_KEY, Campaign.LiveFreemiumOrDie);
            ItemShuffle = GetSlotSetting(ITEM_SHUFFLE_KEY, ItemShuffle.Shuffled);
            DeathLink = GetSlotSetting(DEATH_LINK_KEY, false);
            Seed = GetSlotSetting(SEED_KEY, "0");
            MultiworldVersion = GetSlotSetting(MULTIWORLD_VERSION_KEY, "");
        }

        private int GetSlotSetting(IEnumerable<string> keys, int defaultValue)
        {
            foreach (var key in keys)
            {
                var value = GetSlotSetting(key, defaultValue);
                if (value != defaultValue)
                {
                    return value;
                }
            }

            return defaultValue;
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
        None = 0,
        Region = 1,
        Coin = 2,
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

    public enum ItemShuffle
    {
        Disabled = 0,
        Shuffled = 1,
    }
}
