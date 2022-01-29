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
        public string internalId;
        public string primaryKey;
        public string providerId;
        public SType resourceType;
       
#if ODIN_INSPECTOR
        [InlineEditor]
        [HideLabel]
        [ShowIf(nameof(HasUnityResource))]
#endif        
        public Object resource;
        
#if ODIN_INSPECTOR
        [ShowIf(nameof(HasDependencies))]
#endif
        public List<ResourceLocationData> dependencies = new List<ResourceLocationData>();

        public bool HasUnityResource => resource != null;
        
        public bool HasDependencies => dependencies.Count > 0;
        
        [HideInInspector]
        public IResourceLocation location;

        public override string ToString()
        {
            return $"\t\t InternalId: {internalId} | Key: {primaryKey} | Provider: {providerId} | Type: {resourceType}";
        }

        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return true;
            
            var result = !string.IsNullOrEmpty(internalId) && internalId.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(primaryKey) && primaryKey.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(providerId) && providerId.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;

            return result;
        }
    }
}