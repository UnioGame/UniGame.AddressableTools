using UniCore.Runtime.ProfilerTools;
using UniGame.UniNodes.GameFlow.Runtime;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using global::UniGame.AddressableTools.Runtime;
    using UniModules.UniGame.Core.Runtime.DataFlow;
    using global::UniGame.Core.Runtime;
    using UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.U2D;

    
    [Serializable]
    public class AddressableSpriteAtlasService : GameService, IAddressableAtlasService
    {
        #region inspector

#if ODIN_INSPECTOR
        [InlineProperty]
        [HideLabel]
#endif
        [SerializeField]
        public AddressableAtlasSettings configuration;
        
#if ODIN_INSPECTOR
        [InlineProperty]
        [Searchable]
#endif
        public List<SpriteAtlasHandle> activeHandles = new List<SpriteAtlasHandle>();
        
        #endregion

        private AddressblesAtlasesTagsMap _atlasesReferenceMap;
        
        private Dictionary<string, SpriteAtlasHandle> _loadedAtlases = new Dictionary<string, SpriteAtlasHandle>(8);
        private Dictionary<string, SpriteAtlas> _registeredAtlases = new Dictionary<string, SpriteAtlas>(8);
        private Dictionary<string, UnionLifeTime> _atlasLifeTime = new Dictionary<string, UnionLifeTime>(8);
        

        public AddressableSpriteAtlasService Initialize(AddressableAtlasSettings data)
        {
            configuration = data;
            _atlasesReferenceMap = configuration.atlasesTagsMap;

            BindToAtlasManager();

            PreloadAtlases()
                .AttachExternalCancellation(LifeTime.CancellationToken)
                .Forget();

            UpdateEditorAtlasMode();

            LifeTime.AddCleanUpAction(CleanUp);

            Complete();
            
            return this;
        }

        public void BindAtlasLifeTime(string atlasTag, ILifeTime lifeTime)
        {
            var atlasLifeTime = GetAtlasLifeTime(atlasTag);
            atlasLifeTime.Add(lifeTime);
        }

        public void Validate()
        {
            activeHandles.RemoveAll(x => x.spriteAtlas == null);
        }

        private void UpdateEditorAtlasMode()
        {
#if UNITY_EDITOR
            if (!configuration.isFastMode) return;

            foreach (var atlasPair in _atlasesReferenceMap)
            {
                RegisterAtlas(atlasPair.Value.assetReference.editorAsset);
            }
#endif
        }

        public UnionLifeTime GetAtlasLifeTime(string atlasTag)
        {
            if (_atlasLifeTime.TryGetValue(atlasTag, out var atlasLifeTime))
                return atlasLifeTime;

            atlasLifeTime = new UnionLifeTime();
            atlasLifeTime.AddCleanUpAction(() => UnloadAtlas(atlasTag));
            LifeTime.AddDispose(atlasLifeTime);

            _atlasLifeTime[atlasTag] = atlasLifeTime;

            return atlasLifeTime;
        }

        private async UniTask PreloadAtlases()
        {
            var preload = configuration.preload;
            var preloadAtlases = configuration.preloadAtlases;
            if (preload)
                await UniTask.WhenAll(preloadAtlases.Select(x => LoadAtlas(x.tag, x.assetReference)));
        }

        private void BindToAtlasManager()
        {
            SpriteAtlasManager.atlasRequested -= ProceedSpriteAtlasRequested;
            SpriteAtlasManager.atlasRequested += ProceedSpriteAtlasRequested;
            SpriteAtlasManager.atlasRegistered -= OnSpriteAtlasRegistered;
            SpriteAtlasManager.atlasRegistered += OnSpriteAtlasRegistered;
        }

        private void CleanUp()
        {
            SpriteAtlasManager.atlasRequested -= ProceedSpriteAtlasRequested;
            SpriteAtlasManager.atlasRegistered -= OnSpriteAtlasRegistered;

            activeHandles.Clear();
        }

        public void ProceedSpriteAtlasRequested(string tag, Action<SpriteAtlas> atlasAction)
        {
            GetSpriteAtlas(tag, atlasAction)
                .AttachExternalCancellation(LifeTime.CancellationToken)
                .Forget();
        }

        private async UniTask<SpriteAtlas> GetSpriteAtlas(string tag, Action<SpriteAtlas> atlasAction)
        {
            var atlas = await LoadAtlas(tag);

            if (atlas)
            {
                atlasAction(atlas);
                GameLog.Log($"ATLAS: Register NEW TAG : {tag}", Color.magenta);
                return atlas;
            }

            GameLog.LogError($"ATLAS: Null Atlas Result by TAG {tag}");
            return atlas;
        }

        private async UniTask<SpriteAtlas> LoadAtlas(string tag)
        {
            if (_atlasesReferenceMap.TryGetValue(tag, out var atlasReference) == false)
                return null;

            var assetReference = atlasReference.assetReference;
            var guid = assetReference.AssetGUID;

            GameLog.Log($"ATLAS: OnSpriteAtlasRequested : TAG {tag} GUID {guid}", Color.magenta);

            var spriteAtlas = await LoadAtlas(tag, assetReference);
            return spriteAtlas;
        }

        private async UniTask<SpriteAtlas> LoadAtlas(string tag, AssetReferenceT<SpriteAtlas> assetReference)
        {
            if (_registeredAtlases.TryGetValue(tag, out var registeredAtlas))
                return registeredAtlas;

            var cached = LoadCached(tag);
            if (cached.result)
                return cached.atlas;

            UnloadAtlas(tag);

            var atlasLifeTime = GetAtlasLifeTime(tag);
            var spriteAtlas = await assetReference.LoadAssetTaskAsync(atlasLifeTime);
            var handle = new SpriteAtlasHandle()
            {
                guid = assetReference.AssetGUID,
                tag = tag,
                spriteAtlas = spriteAtlas
            };

            _loadedAtlases[tag] = handle;

            AddEditorHandle(handle);

            return handle.spriteAtlas;
        }

        [Conditional("UNITY_EDITOR")]
        private void AddEditorHandle(SpriteAtlasHandle handle)
        {
#if UNITY_EDITOR
            if (activeHandles.Contains(handle) || activeHandles.Any(x => x.tag == handle.tag))
                return;
            
            activeHandles.Add(handle);
            GetAtlasLifeTime(handle.tag).AddCleanUpAction(() => activeHandles.Remove(handle));
#endif
        }

        private (bool result, SpriteAtlas atlas) LoadCached(string tag)
        {
            if (!_loadedAtlases.TryGetValue(tag, out var atlasHandle))
                return (false, null);

            var atlas = atlasHandle.spriteAtlas;
            var hasAtlas = atlas != null;
            return (hasAtlas, atlas);
        }

        private void UnloadAtlas(string tag)
        {
            if (_atlasLifeTime.TryGetValue(tag, out var atlasHandle))
                atlasHandle.Dispose();
            _loadedAtlases.Remove(tag);
            _registeredAtlases.Remove(tag);
            _atlasLifeTime.Remove(tag);
        }

        private void OnSpriteAtlasRegistered(SpriteAtlas atlas)
        {
#if UNITY_EDITOR
            GameLog.Log($"ATLAS: SpriteAtlasConfiguration : {nameof(OnSpriteAtlasRegistered)} : {atlas.tag}",
                Color.magenta);
#endif
            _registeredAtlases[atlas.tag] = atlas;
        }


#if UNITY_EDITOR
        private static MethodInfo _registerMethod;
        private const string AtlasRegisterMethodName = "Register";

        private static MethodInfo RegisterMethod => (_registerMethod = _registerMethod == null
            ? typeof(SpriteAtlasManager).GetMethod(AtlasRegisterMethodName,
                BindingFlags.Static | BindingFlags.NonPublic)
            : _registerMethod);

        [Conditional("UNITY_EDITOR")]
        private void RegisterAtlas(SpriteAtlas atlas) => RegisterMethod?.Invoke(null, new object[] { atlas });

#endif
    }
}