namespace UniGame.AddressableTools.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using UniGame.Runtime.ObjectPool.Extensions;
    using UniModules.UniCore.Runtime.DataFlow;
    using UniModules.UniGame.Core.Runtime.DataFlow.Extensions;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Serialization;
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
        private const string TransformData = "transform data";
        
        public List<AddressableInstance> links = new List<AddressableInstance>();

        public bool createOnStart = true;
        public bool createOnEnable = false;
        public bool unloadOnDisable = true;
        public bool unloadOnDestroy = true;
        public bool useSceneLifeTime = false;
        public bool spawnUnderParent = false;
        
        public Transform parent;
        
#if ODIN_INSPECTOR
        [BoxGroup(TransformData)]
#endif
        public bool overridePosition = false;
        
#if ODIN_INSPECTOR
        [BoxGroup(TransformData)]
#endif
        public bool overrideScale = false;
        
#if ODIN_INSPECTOR
        [BoxGroup(TransformData)]
        [ShowIf(nameof(overrideScale))]
#endif
        public bool inheritFromParentScale = true;
        
#if ODIN_INSPECTOR
        [BoxGroup(TransformData)]
        [ShowIf(nameof(overrideScale))]
#endif
        public Vector3 scale = Vector3.one;

        [FormerlySerializedAs("useSourcePosition")]
        public bool inheritPosition = true;

        private List<GameObject> _runtime = new();
        private List<Object> _runtimeAssets = new();
        private LifeTime _lifeTime;

        // Start is called before the first frame update
        public void Start()
        {
            if (createOnEnable || !createOnStart) return;
            Create().Forget();
        }

        public void OnEnable()
        {
            if (!createOnEnable) return;
            Create().Forget();
        }

        public void OnDisable()
        {
            if (!unloadOnDisable) return;
            Destroy();
        }

        public void OnDestroy()
        {
            if (!unloadOnDestroy) return;
            Destroy();
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void Reload()
        {
            Destroy();
            Create().Forget();
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void Destroy()
        {
            _lifeTime?.Release();
            _lifeTime = null;
            foreach (var assetItem in _runtime)
            {
                if (assetItem == null) continue;
                assetItem.Despawn();
            }
            foreach (var assetItem in _runtimeAssets)
            {
                if (assetItem == null) continue;
                Destroy(assetItem);
            }
            _runtimeAssets.Clear();
            _runtime.Clear();
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public async UniTask Create()
        {
            _lifeTime?.Release();
            _lifeTime = new LifeTime();
            
            var referencesTasks = links
                .Where(x => x.enabled)
                .Select(x => CreateInstance(x.reference));
            await UniTask.WhenAll(referencesTasks);
        }

        private async UniTask CreateInstance(AssetReference assetReference)
        {
            if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                return;

            var targetParent = spawnUnderParent 
                ? parent == null 
                    ? transform 
                    : parent : null;
            
            var sceneLifeTime = useSceneLifeTime
                ? SceneLifeTimeExtension.GetActiveSceneLifeTime()
                : _lifeTime;
            
            var targetPosition = inheritPosition 
                ? targetParent == null 
                    ? transform.position 
                    : targetParent.position 
                : Vector3.zero;
            
            var scaleValue = overrideScale 
                ? inheritFromParentScale
                    ? targetParent == null 
                        ? Vector3.one
                        : targetParent.localScale
                    : scale
                : Vector3.one;
            
            var asset = await assetReference.LoadAssetTaskAsync<Object>(sceneLifeTime);
            if (asset is GameObject gameObjectAsset)
            {
                var runtimeAsset = gameObjectAsset.Spawn(targetPosition, Quaternion.identity, targetParent);
                runtimeAsset.transform.localScale = scaleValue;
                runtimeAsset.SetActive(true);
                _runtime.Add(runtimeAsset);
            }
            else
            {
                var item = Instantiate(asset);
                _runtimeAssets.Add(item);
            }
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