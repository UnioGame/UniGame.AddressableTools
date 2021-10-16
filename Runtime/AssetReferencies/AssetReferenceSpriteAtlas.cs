namespace UniModules.UniGame.AddressableTools.Runtime.AssetReferencies
{
    using System;
    using UnityEngine.AddressableAssets;
    using UnityEngine.U2D;

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
