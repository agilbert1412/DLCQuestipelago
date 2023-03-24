using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using Core;
using DLCLib;
using Microsoft.Xna.Framework.Content;
using SpriteSheetRuntime;

namespace DLCQuestipelago.DualContentManager
{
    public class DLCDualAssetManager
    {
        private ManualLogSource _console;
        private DLCDualContentManager _contentManager;

        public SpriteSheet BaseCharacterSpriteSheet;
        public SpriteSheet BaseObjectSpriteSheet;
        public SpriteSheet DlcCampaignObjectSpriteSheet;
        // public SpriteSheet DlcCampaignCharacterSpriteSheet;
        public SpriteSheet DlcCampaignDLCSpriteSheet;
        public SpriteSheet LfodCampaignObjectSpriteSheet;
        public SpriteSheet LfodCampaignCharacterSpriteSheet;
        public SpriteSheet LfodCampaignDLCSpriteSheet;

        public DLCDualAssetManager(ManualLogSource console, DLCDualContentManager contentManager)
        {
            _console = console;
            _contentManager = contentManager;

            LoadBaseSpriteSheets();
            LoadCampaignSpriteSheets(ContentSource.DlcCampaign, out DlcCampaignObjectSpriteSheet, out _, out DlcCampaignDLCSpriteSheet);
            LoadCampaignSpriteSheets(ContentSource.LfodCampaign, out LfodCampaignObjectSpriteSheet, out LfodCampaignCharacterSpriteSheet, out LfodCampaignDLCSpriteSheet);
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
                objectSpriteSheet = _contentManager.Load<SpriteSheet>(campaignObjectSpriteSheetPath);
            }

            var campaignCharacterSpriteSheet = Path.Combine("texture", "campaignCharacterSpriteSheet");
            if (_contentManager.HasAsset(campaignCharacterSpriteSheet, source))
            {
                characterSpriteSheet = _contentManager.Load<SpriteSheet>(campaignCharacterSpriteSheet);
            }

            dlcSpriteSheet = _contentManager.Load<SpriteSheet>(Path.Combine("texture", "dlcSpriteSheet"));
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
    }
}
