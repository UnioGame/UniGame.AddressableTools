using System.Diagnostics;
using UniCore.Runtime.ProfilerTools;
using UniModules.UniGame.Core.Runtime.DataFlow;
using UniModules.UniGame.Core.Runtime.DataFlow.Extensions;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if UNITY_EDITOR
    using System.Reflection;
#endif
    using AssetReferencies;
    using Core.Runtime.ScriptableObjects;
    using Cysharp.Threading.Tasks;
    using UniModules.UniCore.Runtime.Attributes;
    using UniModules.UniCore.Runtime.DataFlow;
    using UniModules.UniCore.Runtime.Rx.Extensions;
    using UniModules.UniGame.AddressableTools.Runtime.Extensions;
    using UniRx;
    using UnityEngine.U2D;

    [CreateAssetMenu(menuName = "UniGame/Addressables/SpriteAtlasConfiguration",fileName = nameof(AddressableSpriteAtlasConfiguration))]
    public class AddressableSpriteAtlasConfiguration : 
        LifetimeScriptableObject, 
        IAddressableSpriteAtlasHandler
    {
        #region inspector
        
        [SerializeField]
        public List<AssetReferenceSpriteAtlas> immortalAtlases = new List<AssetReferenceSpriteAtlas>();
        
        [SerializeField]
        public AddressblesAtlasesTagsMap atlasesTagsMap = new AddressblesAtlasesTagsMap();
        
        [SerializeField]
        public bool preloadImmortalAtlases = true;
        
        [SerializeField]
        public bool useSceneLifeTime = false;

        [SerializeField]
        public bool enableLifeTimeOverride = true;

        [SerializeField]
        [ReadOnlyValue]
        public bool isFastMode;

        #endregion

        private UnionLifeTime _atlasesLifetime;

        private Dictionary<string, UnionLifeTime> _atlasesLifeTimeMap = new Dictionary<string, UnionLifeTime>(128);

        public IDisposable Execute()
        {
            Observable.FromEvent(x => SpriteAtlasManager.atlasRequested += OnSpriteAtlasRequested,
                    x => SpriteAtlasManager.atlasRequested -= OnSpriteAtlasRequested).
                Subscribe().
                AddTo(LifeTime);
            
            Observable.FromEvent(
                    x => SpriteAtlasManager.atlasRegistered += OnSpriteAtlasRegistered,
                    x => SpriteAtlasManager.atlasRegistered -= OnSpriteAtlasRegistered).
                Subscribe().
                AddTo(LifeTime);

            if (preloadImmortalAtlases) {
                //load immortal immediate
                foreach (var referenceSpriteAtlas in immortalAtlases) {
                    referenceSpriteAtlas
                        .LoadAssetTaskAsync(LifeTime)
                        .Forget();
                }
            }
            
#if UNITY_EDITOR
            if (!isFastMode) return this;
            
            foreach (var atlasPair in atlasesTagsMap) {
                RegisterAtlas(atlasPair.Value.assetReference.editorAsset);
            }
#endif

            return this;
        }

        public void BindAtlasesLifeTime(ILifeTime lifeTime, IAddressableAtlasesState atlasesState)
        {
            if (useSceneLifeTime) return;

            foreach (var atlasTag in atlasesState.AtlasTags)
            {
                if(_atlasesLifeTimeMap.TryGetValue(atlasTag,out var unionLifeTime))
                {
                    if (unionLifeTime == _atlasesLifetime)
                    {
                        unionLifeTime = lifeTime.ToUnionLifeTime();
                        unionLifeTime.AddCleanUpAction(() => _atlasesLifeTimeMap.Remove(atlasTag));
                        
                        _atlasesLifeTimeMap[atlasTag] = unionLifeTime;
                        
                        continue;
                    }
                    unionLifeTime.Add(lifeTime);
                }
                
                _atlasesLifeTimeMap[atlasTag] = lifeTime.ToUnionLifeTime();
            }
        }
        
        public void Unload() => _atlasesLifetime.Release();

        public async UniTask<bool> RequestSpriteAtlas(string guid)
        {
#if UNITY_EDITOR
            var atlas = await LoadAtlas(guid);
            if (atlas == null)
                return false;
            RegisterAtlas(atlas);
            return true; 
#endif
            return false;
        }

        [ContextMenu("Validate")]
        public void Validate()
        {
#if UNITY_EDITOR
            immortalAtlases.RemoveAll(x => x == null || x.editorAsset == null);

            var keys = atlasesTagsMap.Keys.ToList();
            foreach (var key in keys) {
                var reference = atlasesTagsMap[key];
                if (reference.assetReference == null || reference.assetReference.editorAsset == null)
                    atlasesTagsMap.Remove(key);
            }
#endif
        }

        public void OnSpriteAtlasRegistered(SpriteAtlas atlas)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"SpriteAtlasConfiguration : {nameof(OnSpriteAtlasRegistered)} : {atlas.tag}");
#endif            
        }

        private async void OnSpriteAtlasRequested(string tag, Action<SpriteAtlas> atlasAction)
        {
            if (atlasesTagsMap.TryGetValue(tag, out var atlasReference) == false)
                return;

            var assetReference = atlasReference.assetReference;
            var atlas = await LoadAtlas(assetReference.AssetGUID);
            if (atlas == null)
                return;
            
            atlasAction(atlas);
        }

        private async UniTask<SpriteAtlas> LoadAtlas(string guid)
        {
            var atlasReferencePair = atlasesTagsMap
                .FirstOrDefault(x => x.Value.assetReference.AssetGUID == guid);
            
            
            var assetReference = atlasReferencePair.Value.assetReference;
            if (assetReference == null)
                return null;
            
            var atlasReference = atlasReferencePair.Value;
            var tag            = atlasReferencePair.Key;
            
            GameLog.Log($"ATLAS: OnSpriteAtlasRequested : TAG {tag} GUID {guid}", Color.blue);

            var isImmortal = immortalAtlases.FirstOrDefault(x => x.AssetGUID == guid) != null;
            var lifetime = GetAtlasLifeTime(tag,isImmortal);
            var result   = await assetReference.LoadAssetTaskAsync(lifetime);
            
            if (result == null) {
                GameLog.LogError($"ATLAS: Null Atlas Result by TAG {tag}");
            }
            else {
                lifetime.AddCleanUpAction(() => GameLog.Log($"ATLAS: LifeTime Finished : {tag}",Color.blue));
                GameLog.Log($"ATLAS: Register NEW TAG : {tag}");
            }

            return result;
        }

        private ILifeTime GetAtlasLifeTime(string atlasTag,bool immortal)
        {
            if (immortal) return LifeTime;

            if (enableLifeTimeOverride == false) return _atlasesLifetime;
            
            if (useSceneLifeTime) return SceneManager.GetActiveScene().GetSceneLifeTime();

            if (_atlasesLifeTimeMap.TryGetValue(atlasTag, out var atlasLifeTime))
                return atlasLifeTime;
            
            return _atlasesLifetime;
        }
        
        protected override void OnActivate()
        {
            _atlasesLifetime?.Terminate();
            _atlasesLifetime = new UnionLifeTime();
            _lifeTimeDefinition.AddCleanUpAction(() => _atlasesLifetime.Release());

            foreach (var atlasItem in atlasesTagsMap)
            {
                _atlasesLifeTimeMap[atlasItem.Key] = _atlasesLifetime;
            }
        }

#if UNITY_EDITOR
        
        private static MethodInfo _registerMethod;
        
        private static MethodInfo RegisterMethod => (_registerMethod = _registerMethod == null ? typeof(SpriteAtlasManager).
                GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic) : _registerMethod);

        [Conditional("UNITY_EDITOR")]
        private void RegisterAtlas(SpriteAtlas atlas) => RegisterMethod?.Invoke(null, new object[] {atlas});
        
#endif
    }
}
