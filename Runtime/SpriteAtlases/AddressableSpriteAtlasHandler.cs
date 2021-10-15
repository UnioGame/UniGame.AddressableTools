using UniCore.Runtime.ProfilerTools;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using UniModules.UniCore.Runtime.DataFlow;
    using Extensions;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
    using UniModules.UniGame.Core.Runtime.Interfaces;
    using UnityEngine;
    using UnityEngine.U2D;

    [Serializable]
    public class AddressableSpriteAtlasHandler : ILifeTimeContext, IDisposable
    {
        private AddressableSpriteAtlasConfiguration _configuration;
        private Dictionary<string, SpriteAtlas> _loadedAtlases = new Dictionary<string, SpriteAtlas>(8);
        private Dictionary<string, SpriteAtlas> _registeredAtlases = new Dictionary<string, SpriteAtlas>(8);
        private LifeTimeDefinition _lifeTime;
        private AddressblesAtlasesTagsMap _atlasesReferenceMap;

        public bool IsRuntime { get; private set; }
        
        public ILifeTime LifeTime => _lifeTime??= new LifeTimeDefinition();
        
        public void Bind(AddressableSpriteAtlasConfiguration configuration)
        {
            IsRuntime = true;
            
            _lifeTime??= new LifeTimeDefinition();
            _configuration = configuration;
            _atlasesReferenceMap = _configuration.atlasesTagsMap;
            
            BindToAtlasManager();
            
            PreloadAtlases()
                .AttachExternalCancellation(LifeTime.TokenSource)
                .Forget();

            UpdateEditorAtlasMode();
        }

        public void Dispose() => _lifeTime?.Terminate();

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
            {
                var immortals = await UniTask
                    .WhenAll(preloadAtlases.Select(x => x.LoadAssetTaskAsync(LifeTime)));
                foreach (var atlas in immortals)
                {
                    if (_loadedAtlases.TryGetValue(atlas.tag, out var atlasAsset) && atlasAsset)
                        return;
                    RegisterAtlas(atlas.tag, atlas);
                }
            }
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
        
        public void ProceedSpriteAtlasRequested(string tag, Action<SpriteAtlas> atlasAction)
        {
            GetSpriteAtlas(tag, atlasAction)
                .AttachExternalCancellation(LifeTime.TokenSource)
                .Forget();
        }

        private async UniTask<SpriteAtlas> GetSpriteAtlas(string tag,Action<SpriteAtlas> atlasAction)
        {
            var atlas = await LoadAtlas(tag);

            if (atlas == null)
                GameLog.LogError($"ATLAS: Null Atlas Result by TAG {tag}");
            else
            {
                atlasAction(atlas);
                GameLog.Log($"ATLAS: Register NEW TAG : {tag}");
            }

            return atlas;
        }
        
        private async UniTask<SpriteAtlas> LoadAtlas(string tag)
        {
            if (_atlasesReferenceMap.TryGetValue(tag, out var atlasReference) == false)
                return null;
            
            var assetReference = atlasReference.assetReference;
            if (assetReference == null || assetReference.RuntimeKeyIsValid() == false)
                return null;

            var guid = assetReference.AssetGUID;
            
            GameLog.Log($"ATLAS: OnSpriteAtlasRequested : TAG {tag} GUID {guid}", Color.blue);

            if (_registeredAtlases.TryGetValue(tag, out var registeredAtlas))
            {
                if (!registeredAtlas) 
                    _registeredAtlases.Remove(tag);
                return registeredAtlas;
            }
            
            if (_loadedAtlases.TryGetValue(guid, out var loadedAtlas))
            {
                if (!loadedAtlas) 
                    _loadedAtlases.Remove(guid);
                return loadedAtlas;
            }

            var result = await assetReference.LoadAssetTaskAsync(LifeTime);
            return RegisterAtlas(tag,result);
        }

        private SpriteAtlas RegisterAtlas(string tag, SpriteAtlas atlasAsset)
        {
            //atlasAsset =  UnityEngine.Object.Instantiate(atlasAsset);
            _loadedAtlases[tag] = atlasAsset;
            return atlasAsset;
        }

        private void OnSpriteAtlasRegistered(SpriteAtlas atlas)
        {
#if UNITY_EDITOR
            GameLog.Log($"SpriteAtlasConfiguration : {nameof(OnSpriteAtlasRegistered)} : {atlas.tag}",Color.magenta);
#endif
            
            _registeredAtlases[atlas.tag] = atlas;
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