using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniModules.Editor;
using UniModules.UniGame.AddressableExtensions.Editor;
using UniModules.UniGame.AddressableTools.Runtime.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableDataEditor
{
    [Serializable]
    public class AddressableDependencyView
    {
        private static List<IResourceLocation> emptyData = new List<IResourceLocation>();
        
        #region inspector

        [InlineProperty]
        [SerializeReference]
        [ListDrawerSettings(ListElementLabelName = nameof(IResourceLocation.InternalId))]
        public List<IResourceLocation> references = new List<IResourceLocation>();

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

            references = CreateDependencyData(asset);
        }

        public List<IResourceLocation> CreateDependencyData(Object target)
        {
            if (!target || !target.IsInAnyAddressableAssetGroup())
                return emptyData;

            var guid = target.GetGUID();
            var assetReference = new AssetReference(guid);
                
            AddressableEditorTools.GetResourceLocations(guid, typeof(object), out var locations);
            
            if (locations == null)
                return emptyData;
            
            var dependenciesFromLocations = AddressableRuntimeTools.GatherDependenciesFromLocations(locations);
            return dependenciesFromLocations;
        }

        
        public void Reset()
        {
            references.Clear();
        }
        
    }
}