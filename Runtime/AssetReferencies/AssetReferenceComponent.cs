using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniModules.UniGame.AddressableTools.Runtime.AssetReferencies
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    [Serializable]
    public class AssetReferenceComponentValue<TApi> : 
        AssetReferenceComponent<Component, TApi> where TApi : class
    {
        public AssetReferenceComponentValue(string guid) : base(guid)
        {
        }
    }
    
    [Serializable]
    public class AssetReferenceComponent<TAsset> : 
        AssetReferenceComponent<TAsset, TAsset> where TAsset : Component
    {
        public AssetReferenceComponent(string guid) : base(guid)
        {
        }
    }

    [Serializable]
    public class AssetReferenceComponent<TAsset,TApi> : AssetReferenceT<TAsset> 
        where TAsset : Component
        where TApi : class
    {
#if UNITY_EDITOR
        /// <summary>
        /// Type-specific override of parent editorAsset.  Used by the editor to represent the main asset referenced.
        /// </summary>
        /// <returns>Editor Asset as type TObject, else null</returns>
        public new TAsset editorAsset
        {
            get
            {
                if (CachedAsset != null || string.IsNullOrEmpty(AssetGUID) && CachedAsset is GameObject)
                    return (CachedAsset as GameObject)?.GetComponent<TAsset>();
                
                var assetPath = AssetDatabase.GUIDToAssetPath(AssetGUID);
                var mainType  = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                
                CachedAsset = AssetDatabase.LoadAssetAtPath(assetPath, mainType);
                
                var baseAsset = CachedAsset as GameObject;
                return baseAsset == null 
                    ? null 
                    : baseAsset.GetComponent<TAsset>();
            }
        }
#endif
        
        public AssetReferenceComponent(string guid) : base(guid)
        {
        }
        
        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) 
                as GameObject;
            return prefab?.GetComponent<TApi>() != null;
#else
            return false;
#endif
        }
    }
}