using System;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace UniGame.AddressableTools.Runtime
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.DrawWithUnity]    
#endif

    [Serializable]
    public class AssetReferenceSpriteAtlas : AssetReferenceT<SpriteAtlas>
    {

        public AssetReferenceSpriteAtlas(string guid) : base(guid)
        {

        }
        
    }
}
