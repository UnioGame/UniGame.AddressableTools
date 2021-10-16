
using System.Linq;
using UniModules.UniCore.Runtime.Rx.Extensions;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    using UnityEngine;
    using System.Collections.Generic;
    using Core.Runtime.ScriptableObjects;
    using Cysharp.Threading.Tasks;
    using UniModules.UniCore.Runtime.Attributes;

    [CreateAssetMenu(menuName = "UniGame/Addressables/SpriteAtlasConfiguration",
        fileName = nameof(AddressableSpriteAtlasConfiguration))]
    public class AddressableSpriteAtlasConfiguration :
        LifetimeScriptableObject,
        IAddressableSpriteAtlasHandler
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
        public bool enableLifeTimeOverride = true;
        [SerializeField] 
        [ReadOnlyValue] 
        public bool isFastMode;

        [SerializeField]
#if ODIN_INSPECTOR
        [InlineProperty]
        [HideLabel]
        [VerticalGroup("Active Handles")]
#endif
        public AddressableSpriteAtlasHandler _addressableAtlasHandle;
        
        #endregion
        
        public UniTask Execute()
        {
            Initialize();
            return UniTask.CompletedTask;
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

        protected override void OnActivate() => Initialize();

        private void Initialize()
        {
            if (!Application.isPlaying)
                return;
            _addressableAtlasHandle ??= new AddressableSpriteAtlasHandler();
            _addressableAtlasHandle.Bind(this);
        }
 

    }
}