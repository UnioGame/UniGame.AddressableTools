namespace UniGame.AddressableTools.Runtime
{
    using System;
    using UnityEngine.AddressableAssets;

    [Serializable]
    public class AddressablePreloadValue
    {
        public AssetReferenceGameObject reference;
        public int count = 1;
        public bool prewarm = false;
        public float killDelay = 0.0f;

        public string Label
        {
            get
            {
#if UNITY_EDITOR
                if(reference.editorAsset!=null)
                    return reference.editorAsset.name;
#endif
                return reference.AssetGUID;
            }
        }
    }
}