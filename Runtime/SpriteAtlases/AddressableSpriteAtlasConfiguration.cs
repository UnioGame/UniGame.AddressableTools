using UniCore.Runtime.ProfilerTools;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using System.Diagnostics;
    using UniModules.UniGame.Core.Runtime.DataFlow;
    using UniModules.UniGame.Core.Runtime.DataFlow.Extensions;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AssetReferencies;
    using Core.Runtime.ScriptableObjects;
    using Cysharp.Threading.Tasks;
    using UniModules.UniCore.Runtime.Attributes;
    using Extensions;
    using UnityEngine.U2D;

#if UNITY_EDITOR
    using System.Reflection;
#endif
    
    [CreateAssetMenu(menuName = "UniGame/Addressables/SpriteAtlasConfiguration",
        fileName = nameof(AddressableSpriteAtlasConfiguration))]
    public class AddressableSpriteAtlasConfiguration :
        LifetimeScriptableObject,
        IAddressableSpriteAtlasHandler
    {
        #region inspector

        [SerializeField] public List<AssetReferenceSpriteAtlas> immortalAtlases = new List<AssetReferenceSpriteAtlas>();

        [SerializeField]
#if ODIN_INSPECTOR_3
        [Sirenix.OdinInspector.Searchable]
#endif
        public AddressblesAtlasesTagsMap atlasesTagsMap = new AddressblesAtlasesTagsMap();

        [SerializeField] public bool preloadImmortalAtlases = true;

        [SerializeField] public bool useSceneLifeTime = false;

        [SerializeField] public bool enableLifeTimeOverride = true;

        [SerializeField] [ReadOnlyValue] public bool isFastMode;

        #endregion

        private bool isInitialized = false;
        private UnionLifeTime _atlasesLifetime;
        private Dictionary<string, UnionLifeTime> _atlasesLifeTimeMap = new Dictionary<string, UnionLifeTime>(128);

        private Dictionary<string, SpriteAtlas> _immortalAtlasesMap = new Dictionary<string, SpriteAtlas>(8);

        public async UniTask Execute()
        {
            if (isInitialized)
                return;

            isInitialized = true;
            LifeTime.AddCleanUpAction(() => this.isInitialized = false);

            BindToAtlasManager();

            if (preloadImmortalAtlases)
            {
                _immortalAtlasesMap ??= new Dictionary<string, SpriteAtlas>(8);
                var immortals = await UniTask
                    .WhenAll(immortalAtlases
                        .Select(x => x.LoadAssetTaskAsync(LifeTime)));

                foreach (var atlas in immortals)
                    _immortalAtlasesMap[atlas.tag] = atlas;
            }

#if UNITY_EDITOR
            if (!isFastMode) return;

            foreach (var atlasPair in atlasesTagsMap)
            {
                RegisterAtlas(atlasPair.Value.assetReference.editorAsset);
            }
#endif
        }

        public void BindAtlasesLifeTime(ILifeTime lifeTime, IAddressableAtlasesState atlasesState)
        {
            if (useSceneLifeTime) return;

            foreach (var atlasTag in atlasesState.AtlasTags)
            {
                if (_atlasesLifeTimeMap.TryGetValue(atlasTag, out var unionLifeTime))
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

        public void Unload()
        {
            _atlasesLifetime.Release();
        }

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
            foreach (var key in keys)
            {
                var reference = atlasesTagsMap[key];
                if (reference.assetReference == null || reference.assetReference.editorAsset == null)
                    atlasesTagsMap.Remove(key);
            }
#endif
        }

        public void ProceedSpriteAtlasRequested(string tag, Action<SpriteAtlas> atlasAction)
        {
            OnSpriteAtlasRequested(tag, atlasAction).AttachExternalCancellation(LifeTime.TokenSource).Forget();
        }

        protected override void OnReset()
        {
            Unload();

            isInitialized = false;

            foreach (var atlases in _atlasesLifeTimeMap)
            {
                var tag = atlases.Key;
                if (!_immortalAtlasesMap.ContainsKey(tag))
                    atlases.Value.Terminate();
            }

            _atlasesLifeTimeMap.Clear();
        }


        private void OnSpriteAtlasRegistered(SpriteAtlas atlas)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"SpriteAtlasConfiguration : {nameof(OnSpriteAtlasRegistered)} : {atlas.tag}");
#endif
        }

        private async UniTask OnSpriteAtlasRequested(string tag, Action<SpriteAtlas> atlasAction)
        {
            if (atlasesTagsMap.TryGetValue(tag, out var atlasReference) == false)
                return;

            var assetReference = atlasReference.assetReference;
            var atlas = await LoadAtlas(assetReference.AssetGUID)
                .AttachExternalCancellation(LifeTime.TokenSource);

            if (atlas == null) return;

            atlasAction(atlas);
        }

        private void BindToAtlasManager()
        {
            SpriteAtlasManager.atlasRequested -= ProceedSpriteAtlasRequested;
            SpriteAtlasManager.atlasRequested += ProceedSpriteAtlasRequested;
            SpriteAtlasManager.atlasRegistered -= OnSpriteAtlasRegistered;
            SpriteAtlasManager.atlasRegistered += OnSpriteAtlasRegistered;

            LifeTime.AddCleanUpAction(() =>
            {
                SpriteAtlasManager.atlasRequested -= ProceedSpriteAtlasRequested;
                SpriteAtlasManager.atlasRegistered -= OnSpriteAtlasRegistered;
            });
        }

        private async UniTask<SpriteAtlas> LoadAtlas(string guid)
        {
            var atlasReferencePair = atlasesTagsMap.FirstOrDefault(x => x.Value.assetReference.AssetGUID == guid);
            var assetReference = atlasReferencePair.Value.assetReference;
            if (assetReference == null)
                return null;

            var tag = atlasReferencePair.Key;

            GameLog.Log($"ATLAS: OnSpriteAtlasRequested : TAG {tag} GUID {guid}", Color.blue);

            var isImmortal = immortalAtlases.FirstOrDefault(x => x.AssetGUID == guid) != null;
            var lifetime = GetAtlasLifeTime(tag, isImmortal);
            var result = await assetReference.LoadAssetTaskAsync(lifetime);

            if (result == null)
            {
                GameLog.LogError($"ATLAS: Null Atlas Result by TAG {tag}");
            }
            else
            {
                lifetime.AddCleanUpAction(() => GameLog.Log($"ATLAS: LifeTime Finished : {tag}", Color.blue));
                GameLog.Log($"ATLAS: Register NEW TAG : {tag}");
            }

            return result;
        }

        private ILifeTime GetAtlasLifeTime(string atlasTag, bool immortal)
        {
            if (immortal) return LifeTime;

            if (enableLifeTimeOverride == false) return _atlasesLifetime;

            if (useSceneLifeTime)
                return SceneManager.GetActiveScene()
                    .GetSceneLifeTime();

            return _atlasesLifeTimeMap.TryGetValue(atlasTag, out var atlasLifeTime)
                ? atlasLifeTime
                : _atlasesLifetime;
        }

        protected override void OnActivate()
        {
            _atlasesLifetime?.Terminate();
            _atlasesLifetime = new UnionLifeTime();
            _atlasesLifeTimeMap ??= new Dictionary<string, UnionLifeTime>(128);

            LifeTime.AddCleanUpAction(CleanUp);

            foreach (var atlasItem in atlasesTagsMap)
                _atlasesLifeTimeMap[atlasItem.Key] = _atlasesLifetime;

            BindToAtlasManager();
        }

        private void CleanUp()
        {
            _atlasesLifetime?.Release();
            _atlasesLifeTimeMap?.Clear();
        }

#if UNITY_EDITOR

        private static MethodInfo _registerMethod;

        private static MethodInfo RegisterMethod => (_registerMethod = _registerMethod == null
            ? typeof(SpriteAtlasManager).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)
            : _registerMethod);

        [Conditional("UNITY_EDITOR")]
        private void RegisterAtlas(SpriteAtlas atlas) => RegisterMethod?.Invoke(null, new object[] {atlas});

#endif
    }
}