using UnityEngine;

namespace UniModules.UniGame.AddressableTools.Runtime.AssetReferencies
{
    using System;
    using UnityEngine.AddressableAssets;

    [Serializable]
    public class ScriptableObjectAssetReferenceT<T> : AssetReferenceT<T>
        where T : ScriptableObject
    {
        public ScriptableObjectAssetReferenceT(string guid) : base(guid)
        {
        }
    }
}
