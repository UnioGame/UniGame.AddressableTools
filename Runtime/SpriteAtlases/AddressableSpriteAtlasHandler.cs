using UniCore.Runtime.ProfilerTools;
using UniModules.UniCore.Runtime.Rx.Extensions;
using UnityEngine.AddressableAssets;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using UniModules.UniCore.Runtime.DataFlow;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
    using UniModules.UniGame.Core.Runtime.Interfaces;
    using UnityEngine;
    using UnityEngine.U2D;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    public class AddressableSpriteAtlasHandler : ILifeTimeContext, IDisposable
    {
        private AddressableSpriteAtlasConfiguration _configuration;
        
        private Dictionary<string, SpriteAtlasHandle> _loadedAtlases = new Dictionary<string, SpriteAtlasHandle>(8);
        private Dictionary<string, SpriteAtlas> _registeredAtlases = new Dictionary<string, SpriteAtlas>(8);
        private LifeTimeDefinition _lifeTime;
        private AddressblesAtlasesTagsMap _atlasesReferenceMap;

#if ODIN_INSPECTOR
        [InlineProperty]
#endif
        public List<SpriteAtlasHandle> activeHandles = new List<SpriteAtlasHandle>();

        public bool IsRuntime { get; private set; }

        public ILifeTime LifeTime => _lifeTime ??= new LifeTimeDefinition();

        public AddressableSpriteAtlasHandler Bind(AddressableSpriteAtlasConfiguration configuration)
        {
            IsRuntime = true;

            _lifeTime ??= new LifeTimeDefinition();
            
            _configuration = configuration;
            _atlasesReferenceMap = _configuration.atlasesTagsMap;

            BindToAtlasManager();

            PreloadAtlases()
                .AttachExternalCancellation(LifeTime.TokenSource)
                .Forget();

            UpdateEditorAtlasMode();

            LifeTime.AddCleanUpAction(CleanUp);

            return this;
        }

        public void Dispose() => _lifeTime?.Terminate();

        public void Validate()
        {
            activeHandles.RemoveAll(x => x.spriteAtlas == null);
        }
        
        private void UpdateEditorAtlasMode()
        {
#if UNITY_EDITOR
            if (!_configuration.isFastMode) return;

            foreach (var atlasPair in _atlasesReferenceMap)
            {
                RegisterAtlas(atlasPair.Value.assetReference.editorAsset);
            }
#endif
        }

        private async UniTask PreloadAtlases()
        {
            var preload = _configuration.preloadImmortalAtlases;
            var preloadAtlases = _configuration.immortalAtlases;
            if (preload)
                await UniTask.WhenAll(preloadAtlases.Select(x => LoadAtlas(x.tag,x.assetReference)));
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
                .AttachExternalCancellation(LifeTime.TokenSource)
                .Forget();
        }

        private async UniTask<SpriteAtlas> GetSpriteAtlas(string tag, Action<SpriteAtlas> atlasAction)
        {
            var atlas = await LoadAtlas(tag);

            if (atlas)
            {
                atlasAction(atlas);
                GameLog.Log($"ATLAS: Register NEW TAG : {tag}",Color.magenta);
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
            {
                AddEditorHandle(cached.handle);
                return cached.handle.spriteAtlas;
            }

            UnloadAtlas(tag);

            var handle = await new SpriteAtlasHandle().Load(assetReference);
            _loadedAtlases[tag] = handle;

            LifeTime.AddCleanUpAction(() => UnloadAtlas(tag));
            handle.LifeTime.AddCleanUpAction(() => RemoveCached(tag));
            
            AddEditorHandle(handle);

            return handle.spriteAtlas;
        }

        [Conditional("UNITY_EDITOR")]
        private void AddEditorHandle(SpriteAtlasHandle handle)
        {
#if UNITY_EDITOR
            if (activeHandles.Contains(handle))
                return;
            activeHandles.Add(handle);
            handle.LifeTime.AddCleanUpAction(() => activeHandles.Remove(handle));
#endif
        }

        private (bool result, SpriteAtlasHandle handle) LoadCached(string tag)
        {
            if (!_loadedAtlases.TryGetValue(tag, out var atlasHandle)) 
                return (false, null);

            var atlas = atlasHandle.spriteAtlas;
            var hasAtlas = atlas != null;
            return (hasAtlas, atlasHandle);
        }

        private void UnloadAtlas(string tag)
        {
            if (_loadedAtlases.TryGetValue(tag, out var atlasHandle))
                atlasHandle.Dispose();
        }

        private void RemoveCached(string tag)
        {
            _loadedAtlases.Remove(tag);
            _registeredAtlases.Remove(tag);
        }

        private void OnSpriteAtlasRegistered(SpriteAtlas atlas)
        {
#if UNITY_EDITOR
            GameLog.Log($"ATLAS: SpriteAtlasConfiguration : {nameof(OnSpriteAtlasRegistered)} : {atlas.tag}",Color.magenta);
#endif
            _registeredAtlases[atlas.tag] = atlas;
        }


#if UNITY_EDITOR
        private static MethodInfo _registerMethod;
        private const string AtlasRegisterMethodName = "Register";

        private static MethodInfo RegisterMethod => (_registerMethod = _registerMethod == null
            ? typeof(SpriteAtlasManager).GetMethod(AtlasRegisterMethodName, BindingFlags.Static | BindingFlags.NonPublic)
            : _registerMethod);

        [Conditional("UNITY_EDITOR")]
        private void RegisterAtlas(SpriteAtlas atlas) => RegisterMethod?.Invoke(null, new object[] {atlas});

#endif
    }
}