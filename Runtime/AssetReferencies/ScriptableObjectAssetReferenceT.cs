using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniGame.AddressableTools.Runtime
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
