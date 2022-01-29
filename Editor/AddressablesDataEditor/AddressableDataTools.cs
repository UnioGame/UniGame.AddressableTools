using System;
using System.Collections.Generic;
using System.Linq;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor.Window;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor
{
    public static class AddressableDataTools
    {
        public const string localBuildPath = "com.unity.addressables";
        
        public static AddressableAssetEntryData CreateEntryData(AddressableAssetEntry entry,bool selectDependencies = true)
        {
            if (entry == null) return new AddressableAssetEntryData();
            
            var parent = entry.parentGroup;
            var schema = parent.GetSchema<BundledAssetGroupSchema>();
            var buildPath = schema == null ? string.Empty : schema.BuildPath.GetValue(parent.Settings);
            var isLocal = buildPath.IndexOf(localBuildPath,StringComparison.InvariantCultureIgnoreCase) >= 0;
            
            var result = new AddressableAssetEntryData()
            {
                guid = entry.guid,
                isRemote = !isLocal,
                address = entry.address,
                labels = entry.labels.ToList(),
                groupName = entry.parentGroup.Name,
                readOnly = entry.ReadOnly,
                buildPath = buildPath,
                asset = entry.MainAsset,
                dependenciesLocations = selectDependencies ? CreateDependencies(entry) : new List<ResourceLocationData>(),
            };

            return result;
        }

        public static List<ResourceLocationData> CreateDependencies(AddressableAssetEntry assetEntry)
        {
            var locationsData = new List<ResourceLocationData>();
            var locations = Addressables.LoadResourceLocationsAsync(assetEntry.guid);
            var resourceLocations = locations.WaitForCompletion();

            foreach (var location in resourceLocations)
            {
                foreach (var dependency in location.Dependencies)
                {
                    var locationData = CreateResourceLocationInfo(dependency);
                    locationsData.Add(locationData);
                }
            }

            return locationsData;
        }

        public static List<ResourceLocationData> CreateResourceLocationInfo(IList<IResourceLocation> locations)
        {
            var result = new List<ResourceLocationData>();
            if (locations == null)
                return result;
            
            foreach (var location in locations)
            {
                result.Add(CreateResourceLocationInfo(location));
            }

            return result;
        }
        
        public static ResourceLocationData CreateResourceLocationInfo(IResourceLocation location)
        {
            var result = new ResourceLocationData()
            {
                location = location,
                internalId = location.InternalId,
                primaryKey = location.PrimaryKey,
                providerId = location.ProviderId,
                dependencies = CreateResourceLocationInfo(location.Dependencies),
                resourceType = location.ResourceType,
                resource = location.Data as Object,
            };
            return result;
        }
    }
}