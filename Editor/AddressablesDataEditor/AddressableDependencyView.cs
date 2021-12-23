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
        public Object asset;
        
        [Space]
        [InlineProperty]
        [SerializeReference]
        [ListDrawerSettings(ListElementLabelName = nameof(IResourceLocation.InternalId))]
        public List<IResourceLocation> references = new List<IResourceLocation>();

        #endregion

        [Button]
        public void Update()
        {
            Clear();
            
            if (!asset) return;
            UpdateView(asset);
        }
        
        public void UpdateView(Object target)
        {
            asset = target;
            
            Clear();

            references = CreateDependencyData(asset);
        }

        public List<IResourceLocation> CreateDependencyData(Object target)
        {
            if (!target || !target.IsInAnyAddressableAssetGroup())
                return emptyData;

            var assetReference = new AssetReference(target.GetGUID());
                
            AddressableEditorTools.GetResourceLocations(assetReference, typeof(object), out var locations);
            if (locations == null)
                return emptyData;
            
            var dependenciesFromLocations = AddressableRuntimeTools.GatherDependenciesFromLocations(locations);
            return dependenciesFromLocations;
        }



        public void Clear()
        {
            references.Clear();
        }
        
    }
}