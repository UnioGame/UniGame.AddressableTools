using System;
using System.Collections.Generic;
using UniModules.UniGame.Core.Runtime.SerializableType;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableDataEditor
{
    [Serializable]
    public class ResourceLocationData
    {
        public string id;
        public string key;
        public string providerId;
        public SType resourceType;
        public List<ResourceLocationData> dependencies;

        [HideInInspector]
        public IResourceLocation location;
    }
}