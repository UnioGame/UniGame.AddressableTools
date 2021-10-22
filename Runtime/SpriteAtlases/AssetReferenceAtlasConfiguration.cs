﻿namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using System;
    using UnityEngine.AddressableAssets;

    [Serializable]
    public class AssetReferenceAtlasConfiguration : AssetReferenceT<AddressableSpriteAtlasAsset>
    {
        public AssetReferenceAtlasConfiguration(string guid) : base(guid)
        {
        }
    }
}