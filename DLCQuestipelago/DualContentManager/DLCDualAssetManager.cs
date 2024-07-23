using System;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;

namespace DLCQuestipelago.DualContentManager
{
    public class DLCDualAssetManager
    {
        private ILogger _logger;
        private DLCDualContentManager _contentManager;

        public SpriteSheet BaseCharacterSpriteSheet;
        public SpriteSheet BaseObjectSpriteSheet;
        public SpriteSheet DlcCampaignObjectSpriteSheet;
        // public SpriteSheet DlcCampaignCharacterSpriteSheet;
        public SpriteSheet DlcCampaignDLCSpriteSheet;
        public SpriteSheet LfodCampaignObjectSpriteSheet;
        public SpriteSheet LfodCampaignCharacterSpriteSheet;
        public SpriteSheet LfodCampaignDLCSpriteSheet;

        private SpriteSheet[] _dlcSpriteSheets;
        private SpriteSheet[] _allSpriteSheets;

        public DLCDualAssetManager(ILogger logger, DLCDualContentManager contentManager)
        {
            _logger = logger;
            _contentManager = contentManager;

            LoadBaseSpriteSheets();
            LoadCampaignSpriteSheets(ContentSource.DlcCampaign, out DlcCampaignObjectSpriteSheet, out _, out DlcCampaignDLCSpriteSheet);
            LoadCampaignSpriteSheets(ContentSource.LfodCampaign, out LfodCampaignObjectSpriteSheet, out LfodCampaignCharacterSpriteSheet, out LfodCampaignDLCSpriteSheet);
            _dlcSpriteSheets = new[] { DlcCampaignDLCSpriteSheet, LfodCampaignDLCSpriteSheet };
            _allSpriteSheets = new[]
            {
                BaseCharacterSpriteSheet, BaseObjectSpriteSheet, DlcCampaignObjectSpriteSheet,
                DlcCampaignDLCSpriteSheet, LfodCampaignObjectSpriteSheet, LfodCampaignCharacterSpriteSheet,
                LfodCampaignDLCSpriteSheet,
            };
        }

        private void LoadBaseSpriteSheets()
        {
            BaseObjectSpriteSheet = _contentManager.Load<SpriteSheet>(Path.Combine("texture", "objectSpriteSheet"), ContentSource.Base);
            BaseCharacterSpriteSheet = _contentManager.Load<SpriteSheet>(Path.Combine("texture", "characterSpriteSheet"), ContentSource.Base);
        }

        private void LoadCampaignSpriteSheets(ContentSource source, out SpriteSheet objectSpriteSheet,
            out SpriteSheet characterSpriteSheet, out SpriteSheet dlcSpriteSheet)
        {
            objectSpriteSheet = null;
            characterSpriteSheet = null;
            var campaignObjectSpriteSheetPath = Path.Combine("texture", "campaignObjectSpriteSheet");
            if (_contentManager.HasAsset(campaignObjectSpriteSheetPath, source))
            {
                objectSpriteSheet = _contentManager.Load<SpriteSheet>(campaignObjectSpriteSheetPath, source);
            }

            var campaignCharacterSpriteSheet = Path.Combine("texture", "campaignCharacterSpriteSheet");
            if (_contentManager.HasAsset(campaignCharacterSpriteSheet, source))
            {
                characterSpriteSheet = _contentManager.Load<SpriteSheet>(campaignCharacterSpriteSheet, source);
            }

            dlcSpriteSheet = _contentManager.Load<SpriteSheet>(Path.Combine("texture", "dlcSpriteSheet"), source);
        }

        public SpriteSheet[] GetSpriteSheetsByName(string name)
        {
            switch (name)
            {
                case "Character":
                    return new[] { BaseCharacterSpriteSheet };
                case "Object":
                    return new[] { BaseObjectSpriteSheet };
                case "CampaignCharacter":
                    return new[] { LfodCampaignCharacterSpriteSheet };
                default:
                    return new SpriteSheet[0];
            }
        }

        public void FindIcon(string iconName, out Texture2D texture, out Rectangle rectangle)
        {
            foreach (var dlcSpriteSheet in _dlcSpriteSheets)
            {
                if (dlcSpriteSheet.Contains(iconName))
                {
                    texture = dlcSpriteSheet.Texture;
                    rectangle = dlcSpriteSheet.SourceRectangle(iconName);
                    return;
                }
            }

            throw new Exception($"Could not find icon {iconName} in any of the sprite sheets.");
        }
    }
}
