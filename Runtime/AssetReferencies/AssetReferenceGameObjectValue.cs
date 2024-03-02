namespace Game.Runtime.Services.AddressablesSource
{
    using System;
    using UnityEngine.AddressableAssets;

    [Serializable]
    public class AssetReferenceGameObjectValue : AssetReferenceGameObject
    {
        public bool makeImmortal;

        public AssetReferenceGameObjectValue(string guid) : base(guid)
        {
        }
    }
}