using System;
using System.Collections.Generic;
using UniModules.UniGame.Core.Runtime.SerializableType;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    public class ResourceLocationData
    #if ODIN_INSPECTOR
        : ISearchFilterable
    #endif
    {
        private const string localLocation = "local";
        private const string infoGroup = "info";
        
        public string internalId;
        
#if ODIN_INSPECTOR
        [FoldoutGroup(infoGroup)]
#endif
        public string primaryKey;
#if ODIN_INSPECTOR
        [FoldoutGroup(infoGroup)]
#endif
        public string providerId;
#if ODIN_INSPECTOR
        [FoldoutGroup(infoGroup)]
#endif
        public SType resourceType;
#if ODIN_INSPECTOR
        [FoldoutGroup(infoGroup)]
#endif
        public bool isRemote;
#if ODIN_INSPECTOR
        [FoldoutGroup(infoGroup)]
#endif
        public string path;

#if ODIN_INSPECTOR
        [InlineEditor]
        [HideLabel]
        [FoldoutGroup(infoGroup)]
        [ShowIf(nameof(HasUnityResource))]
#endif        
        public Object resource;
        
#if ODIN_INSPECTOR
        [FoldoutGroup(infoGroup)]
        [ShowIf(nameof(HasDependencies))]
#endif
        public List<ResourceLocationData> dependencies = new List<ResourceLocationData>();

        public bool HasUnityResource => resource != null;
        
        public bool HasDependencies => dependencies.Count > 0;
        
        [HideInInspector]
        public IResourceLocation location;

        public override string ToString()
        {
            return $"\t\tInternalId: {internalId} | Key: {primaryKey} | Provider: {providerId} | Type: {resourceType}";
        }

        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return true;
            
            var result = !string.IsNullOrEmpty(internalId) && internalId.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(primaryKey) && primaryKey.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(providerId) && providerId.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= isRemote && nameof(isRemote).IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase)>=0;
            result |= !isRemote && localLocation.IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase)>=0;
            return result;
        }
    }
}