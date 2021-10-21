using System.Linq;
using UniCore.Runtime.ProfilerTools;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using UnityEngine;
    using Core.Runtime.ScriptableObjects;
    
    
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    
    [CreateAssetMenu(menuName = "UniGame/Addressables/SpriteAtlasConfiguration",
        fileName = nameof(AddressableSpriteAtlasAsset))]
    public class AddressableSpriteAtlasAsset :
        LifetimeScriptableObject
    {
        #region inspector

#if ODIN_INSPECTOR
        [InlineProperty]
#endif
        [SerializeField]
        public AddressableAtlasSettings settings;
        
        #endregion

        #region static data

        private static IAddressableAtlasService _atlasService;
        private static object gate = new object();
        public static IAddressableAtlasService AtlasService
        {
            get => _atlasService;
            set
            {
                lock (gate)
                {
                    if (_atlasService != null && _atlasService.LifeTime.IsTerminated == false)
                    {
                        GameLog.LogWarning("AtlasService Already exists");
                        return;
                    }
                    _atlasService = value;
                    _atlasService.LifeTime.AddCleanUpAction(() => _atlasService = null);
                }
            }
        }
        
        #endregion


#if ODIN_INSPECTOR
        [Button]
#endif
        public void Reload()
        {
            Unload();
            AtlasService = new AddressableSpriteAtlasService().Initialize(settings);
        }
        
#if ODIN_INSPECTOR
        [Button]
#endif
        public void Unload()
        {
            AtlasService?.Dispose();
        }

        public void Initialize()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false) return;
#endif
            if (AtlasService == null)
                Reload();
        }
        
        [ContextMenu("Validate")]
        public void Validate()
        {
#if UNITY_EDITOR
            var preloadAtlases = settings.preloadAtlases;
            preloadAtlases.RemoveAll(x => x.assetReference == null || x.assetReference.editorAsset == null);
            preloadAtlases.ForEach(x => x.UpdateTag());

            var map = settings.atlasesTagsMap;
            var keys = map.Keys.ToList();
            foreach (var key in keys)
            {
                var reference = map[key];
                if (reference.assetReference == null || reference.assetReference.editorAsset == null)
                    map.Remove(key);
            }
#endif
        }

        protected sealed override void OnActivate()
        {
            Initialize();
        }

        protected sealed override void OnReset()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false) return;
#endif
            if (settings.disposeOnReset)
                Unload();
        }


    }
}