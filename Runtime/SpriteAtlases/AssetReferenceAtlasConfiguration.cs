using System;
using UnityEngine.AddressableAssets;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public class AssetReferenceAtlasConfiguration : AssetReferenceT<AddressableSpriteAtlasAsset>
    {
        public AssetReferenceAtlasConfiguration(string guid) : base(guid)
        {
        }
    }
}