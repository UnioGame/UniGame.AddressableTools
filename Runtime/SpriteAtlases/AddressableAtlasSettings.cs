using System;
using System.Collections.Generic;
using UniCore.Runtime.Attributes;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public class AddressableAtlasSettings
    {
        #region inspector

        [SerializeField] 
        public List<AtlasReference> preloadAtlases = new List<AtlasReference>();

        [SerializeField]
#if ODIN_INSPECTOR_3
        [Searchable]
#endif
        public AddressblesAtlasesTagsMap atlasesTagsMap = new AddressblesAtlasesTagsMap();

        [SerializeField] 
        public bool preload = true;

        [SerializeField] 
        [ReadOnlyValue] 
        public bool isFastMode;

        [SerializeField]
        public bool disposeOnReset = false;
        
        #endregion
    }
}