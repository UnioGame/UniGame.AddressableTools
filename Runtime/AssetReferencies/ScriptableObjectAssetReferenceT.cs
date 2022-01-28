using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.AssetReferencies
{
    [Serializable]
    public class ScriptableObjectAssetReferenceT<T> : AssetReferenceT<T>
        where T : ScriptableObject
    {
        public ScriptableObjectAssetReferenceT(string guid) : base(guid)
        {
        }
    }
}
