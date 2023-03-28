using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using DLCLib;
using Microsoft.Xna.Framework.Content;

namespace DLCQuestipelago.DualContentManager
{
    public class DLCDualContentManager
    {
        private ManualLogSource _console;

        private ContentManager _baseContentManager;
        private ContentManager _dlcCampaignContentManager;
        private ContentManager _lfodCampaignContentManager;
        private List<string> _baseAssetList;
        private List<string> _dlcCampaignAssetList;
        private List<string> _lfodCampaignAssetList;

        public ContentManager BaseContentManager => _baseContentManager;

        public ContentManager DlcCampaignContentManager => _dlcCampaignContentManager;

        public ContentManager LfodCampaignContentManager => _lfodCampaignContentManager;

        public DLCDualContentManager(IServiceProvider serviceProvider, string rootDirectory, ManualLogSource console)
        {
            _console = console;

            _baseContentManager = new ContentManager(serviceProvider, rootDirectory);
            _baseAssetList = ContentUtils.LoadContentManifest(_baseContentManager, "manifest");

            var campaigns = "campaigns";
            var dlcCampaign = "dlcquest";
            var lfodCampaign = "lfod";

            _dlcCampaignContentManager = new ContentManager(serviceProvider, Path.Combine("Content", campaigns, dlcCampaign));
            _dlcCampaignAssetList = ContentUtils.LoadContentManifest(_dlcCampaignContentManager, "manifest");

            _lfodCampaignContentManager = new ContentManager(serviceProvider, Path.Combine("Content", campaigns, lfodCampaign));
            _lfodCampaignAssetList = ContentUtils.LoadContentManifest(_lfodCampaignContentManager, "manifest");
        }

        public bool HasCampaignAsset(string assetName) => _dlcCampaignAssetList.Contains(assetName) || _lfodCampaignAssetList.Contains(assetName);

        public bool HasAsset(string assetName, ContentSource source)
        {
            switch (source)
            {
                case ContentSource.Base:
                    return _baseAssetList.Contains(assetName);
                case ContentSource.DlcCampaign:
                    return _dlcCampaignAssetList.Contains(assetName);
                case ContentSource.LfodCampaign:
                    return _lfodCampaignAssetList.Contains(assetName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        public T Load<T>(string assetName)
        {
            if (_dlcCampaignAssetList.Contains(assetName))
                return _dlcCampaignContentManager.Load<T>(assetName);
            if (_lfodCampaignAssetList.Contains(assetName))
                return _lfodCampaignContentManager.Load<T>(assetName);
            if (_baseAssetList.Contains(assetName))
                return _baseContentManager.Load<T>(assetName);

            var message = $"Could not load asset: {assetName}.";
            _console.LogError(message);
            throw new ArgumentException(message);
        }

        public T Load<T>(string assetName, ContentSource preference)
        {
            switch (preference)
            {
                case ContentSource.Base:
                    return _baseContentManager.Load<T>(assetName);
                case ContentSource.DlcCampaign:
                    return _dlcCampaignContentManager.Load<T>(assetName);
                case ContentSource.LfodCampaign:
                    return _lfodCampaignContentManager.Load<T>(assetName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(preference), preference, null);
            }
        }
    }

    public enum ContentSource
    {
        Base,
        DlcCampaign,
        LfodCampaign,
    }
}
