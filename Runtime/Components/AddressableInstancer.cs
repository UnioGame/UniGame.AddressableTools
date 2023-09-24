namespace UniGame.AddressableTools.Runtime
{
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using UniGame.Runtime.ObjectPool.Extensions;
    using UniModules.UniGame.Core.Runtime.DataFlow.Extensions;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class AddressableInstancer : MonoBehaviour
    {
        public AssetReference reference;
        public List<AssetReference> references = new List<AssetReference>();
        
        public bool           createOnStart = true;
        public Transform      parent;
        public bool           useParent = true;
    
        // Start is called before the first frame update
        public void Start()
        {
            if (createOnStart)
                Create().Forget();
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public async UniTask Create()
        {
            var referenceTask = CreateInstance(reference);
            var referencesTasks = references
                    .Select(CreateInstance)
                    .Prepend(referenceTask);
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
}
