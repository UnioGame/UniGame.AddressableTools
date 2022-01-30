using System;
using UniModules.UniGame.AddressableTools.Runtime.AssetReferencies;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public struct AtlasReference
    {
        public string tag;
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.OnValueChanged(nameof(UpdateTag))]
#endif
        public AssetReferenceSpriteAtlas assetReference;

        public void UpdateTag()
        {
#if UNITY_EDITOR
            var atlas = assetReference?.editorAsset;
            if (atlas == null)
                return;
            tag = atlas.tag;
#endif
        }
    }
}