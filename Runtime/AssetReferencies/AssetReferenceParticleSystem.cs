using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniGame.AddressableTools.Runtime
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.DontApplyToListElements]
#endif
    [Serializable]
    public class AssetReferenceParticleSystem : AssetReferenceT<ParticleSystem>
    {
        public AssetReferenceParticleSystem(string guid) : base(guid)
        {
        }

        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(ParticleSystem));
            return prefab != null;
#else
            return false;
#endif
        }
    }
}