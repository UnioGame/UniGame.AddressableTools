namespace UniGame.AddressableTools.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using UniGame.Runtime.ObjectPool.Extensions;
    using UniModules.UniGame.Core.Runtime.DataFlow.Extensions;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

#if UNITY_EDITOR
    using UnityEditor;
    using UniModules.Editor;
#endif
    
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    public class AddressableInstancer : MonoBehaviour
    {
        public List<AddressableInstance> links = new List<AddressableInstance>();
        
        public bool           createOnStart = true;
        public Transform      parent;
        public bool           useParent = true;
        public bool           destroyOnReload = true;
        
        private List<GameObject> _runtime = new List<GameObject>();
    
        // Start is called before the first frame update
        public void Start()
        {
            if (!createOnStart)  return;
            Create().Forget();
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void Reload()
        {
            foreach (var asset in _runtime)
            {
                if(asset == null) continue;
                
                if(destroyOnReload)
                    Destroy(asset);
                else
                    asset.Despawn();
            }
            
            _runtime.Clear();
            
            Create().Forget();
        }
        
#if ODIN_INSPECTOR
        [Button]
#endif
        public async UniTask Create()
        {
            var referencesTasks = links
                .Where(x => x.enabled)
                .Select(x => CreateInstance(x.reference));
            await UniTask.WhenAll(referencesTasks);
        }
        
        private async UniTask CreateInstance(AssetReference assetReference)
        {
            if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                return;
            
            var targetParent = useParent ? parent == null ? transform : parent : null;
            var sceneLifeTime = SceneLifeTimeExtension.GetActiveSceneLifeTime();
            var asset        = await assetReference.LoadAssetTaskAsync<Object>(sceneLifeTime);
            var runtimeAsset = asset.Spawn(Vector3.zero, Quaternion.identity, targetParent);
            
            if(runtimeAsset is GameObject runtimeGameObject)
                _runtime.Add(runtimeGameObject);
        }

    }
    
    [Serializable]
#if ODIN_INSPECTOR
    [InlineProperty]
#endif
    public class AddressableInstance
    {
#if ODIN_INSPECTOR
        [HideLabel]
        [HorizontalGroup(Width = 20)]
#endif
        public bool enabled = true;
        
#if ODIN_INSPECTOR
        [HideLabel]
        [HorizontalGroup]
#endif
        public AssetReference reference;
    }
}
