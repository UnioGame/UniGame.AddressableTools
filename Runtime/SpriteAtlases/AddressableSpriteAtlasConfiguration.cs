
namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    using System.Linq;
    using UniModules.UniCore.Runtime.Rx.Extensions;
    using UnityEngine;
    using System.Collections.Generic;
    using Core.Runtime.ScriptableObjects;
    using UniModules.UniCore.Runtime.Attributes;

    [CreateAssetMenu(menuName = "UniGame/Addressables/SpriteAtlasConfiguration",
        fileName = nameof(AddressableSpriteAtlasConfiguration))]
    public class AddressableSpriteAtlasConfiguration :
        LifetimeScriptableObject
    {
        #region inspector

        [SerializeField] 
        public List<AtlasReference> immortalAtlases = new List<AtlasReference>();

        [SerializeField]
#if ODIN_INSPECTOR_3
        [Searchable]
#endif
        public AddressblesAtlasesTagsMap atlasesTagsMap = new AddressblesAtlasesTagsMap();

        [SerializeField] 
        public bool preloadImmortalAtlases = true;
        
        [SerializeField] 
        public bool useSceneLifeTime = false;
        
        [SerializeField] 
        [ReadOnlyValue] 
        public bool isFastMode;

        [SerializeField]
        public bool disposeOnReset = true;
        
        [SerializeField]
#if ODIN_INSPECTOR
        [InlineProperty]
        [HideLabel]
        [VerticalGroup("Active Handles")]
#endif
        public AddressableSpriteAtlasService _addressableAtlasHandle;

#if UNITY_EDITOR
        public static AddressableSpriteAtlasConfiguration AddressableAtlasConfigurationAsset;
#endif
        
        #endregion

        public IAddressableAtlasService AtlasService => _addressableAtlasHandle;
        
        public void Initialize()
        {
#if UNITY_EDITOR
            AddressableAtlasConfigurationAsset = this;
#endif
            _addressableAtlasHandle = new AddressableSpriteAtlasService();
            _addressableAtlasHandle.Initialize(this);
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void Reload()
        {
            Unload();
            Initialize();
        }
        
#if ODIN_INSPECTOR
        [Button]
#endif
        public void Unload()
        {
            _addressableAtlasHandle.Cancel();
            _addressableAtlasHandle = null;
        }
        
        
        [ContextMenu("Validate")]
        public void Validate()
        {
#if UNITY_EDITOR
            _addressableAtlasHandle?.Validate();
            immortalAtlases.RemoveAll(x => x.assetReference == null || x.assetReference.editorAsset == null);
            immortalAtlases.ForEach(x => x.UpdateTag());
            
            var keys = atlasesTagsMap.Keys.ToList();
            foreach (var key in keys)
            {
                var reference = atlasesTagsMap[key];
                if (reference.assetReference == null || reference.assetReference.editorAsset == null)
                    atlasesTagsMap.Remove(key);
            }
#endif
        }

        protected sealed override void OnActivate()
        {
        }

        protected sealed override void OnReset()
        {
            if (!disposeOnReset) return;
            Unload();
        }


    }
}