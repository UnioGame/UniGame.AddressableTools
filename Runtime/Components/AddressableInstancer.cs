namespace UniGame.AddressableTools.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Sirenix.OdinInspector;
    using UniGame.Runtime.ObjectPool.Extensions;
    using UniModules.UniGame.Core.Runtime.DataFlow.Extensions;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

#if UNITY_EDITOR
    using UnityEditor;
    using UniModules.Editor;
#endif
    
    public class AddressableInstancer : MonoBehaviour
    {
        public List<AddressableInstance> links = new List<AddressableInstance>();
        
        public bool           createOnStart = true;
        public Transform      parent;
        public bool           useParent = true;
    
        // Start is called before the first frame update
        public void Start()
        {
            if (!createOnStart)  return;
            Create().Forget();
        }
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
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
            asset.Spawn(Vector3.zero, Quaternion.identity, targetParent);
        }

    }
    
    [Serializable]
    [InlineProperty]
    public class AddressableInstance
    {
        [HideLabel]
        [HorizontalGroup(Width = 20)]
        public bool enabled = true;
        
        [HideLabel]
        [HorizontalGroup]
        public AssetReference reference;
    }
}
