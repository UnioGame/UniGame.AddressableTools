using System;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.AssetReferencies
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
