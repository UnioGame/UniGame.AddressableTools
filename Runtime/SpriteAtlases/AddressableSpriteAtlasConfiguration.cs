
namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if UNITY_EDITOR
    using UnityEditor;
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

        #endregion
        
        
#if ODIN_INSPECTOR
        [InlineProperty]
        [HideLabel]
        [VerticalGroup("Active Handles")]
#endif
        public static AddressableSpriteAtlasService addressableAtlasHandle;

        public IAddressableAtlasService AtlasService => addressableAtlasHandle;
        
        public void Initialize()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
                return;
#endif
            if (addressableAtlasHandle != null) return;
            addressableAtlasHandle = new AddressableSpriteAtlasService();
            addressableAtlasHandle.Initialize(this);
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
            addressableAtlasHandle.Cancel();
            addressableAtlasHandle = null;
            AddressableAtlasConfigurationAsset = null;
        }
        
        [ContextMenu("Validate")]
        public void Validate()
        {
#if UNITY_EDITOR
            addressableAtlasHandle?.Validate();
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
        
        protected sealed override void OnReset()
        {
            if (!disposeOnReset) return;
            Unload();
        }


    }
}