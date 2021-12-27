using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniModules.Editor;
using UniModules.UniGame.AddressableExtensions.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableDataEditor
{
    [Serializable]
    public class AddressableDependencyView
    {
        private static List<ResourceLocationData> emptyData = new List<ResourceLocationData>();
        
        #region inspector

        [InlineProperty]
        [SerializeReference]
        [ListDrawerSettings(ListElementLabelName = nameof(ResourceLocationData.id))]
        public List<ResourceLocationData> references = new List<ResourceLocationData>();

        #endregion

        private Object asset;

        [Button]
        public void Update()
        {
            Reset();
            if (!asset) return;
            Initialize(asset);
        }
        
        public void Initialize(Object target)
        {
            asset = target;
            
            Reset();

            references.Clear();
            references.AddRange(CreateDependencyData(asset));
        }

        public IList<ResourceLocationData> CreateDependencyData(Object target)
        {
            if (!target || !target.IsInAnyAddressableAssetGroup())
                return emptyData;

            var guid = target.GetGUID();
            var locations = AddressableEditorTools.GetAddressableResourceDependencies(guid, typeof(object));

            if (locations == null) return emptyData;

            return AddressableDataTools.CreateResourceLocationInfo(locations);
        }

        
        public void Reset()
        {
            references.Clear();
        }
        
    }
}