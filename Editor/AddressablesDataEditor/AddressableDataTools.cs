namespace UniGame.AddressableTools.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UniModules;
    using UniModules.Editor;
    using UnityEditor;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.AddressableAssets.Settings.GroupSchemas;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using Object = UnityEngine.Object;
    
    public static class AddressableDataTools
    {
        public const string localBuildPath = "com.unity.addressables";
        
        
        public static AddressableAssetEntryData CreateEntryData(AddressableAssetEntry entry,bool selectDependencies = true,bool recursive = false)
        {
            if (entry == null) return new AddressableAssetEntryData();
            
            var parent = entry.parentGroup;
            var schema = GetGroupBuildPath(parent);
            var buildPath = schema.buildPath;
            var isLocal = schema.isLocal;

            var entryDependencies = selectDependencies
                ? GetAddressablesDependencies(entry.address,recursive)
                : new List<AddressableAssetEntryData>();

            var dependencies = selectDependencies 
                ? CreateDependencies(entry) 
                : new List<ResourceLocationData>();
            
            var result = new AddressableAssetEntryData()
            {
                guid = entry.guid,
                isRemote = !isLocal,
                address = entry.address,
                labels = entry.labels.ToList(),
                groupName = entry.parentGroup.Name,
                readOnly = entry.ReadOnly,
                buildPath = buildPath,
                name = Path.GetFileNameWithoutExtension(entry.address),
                asset = entry.MainAsset,
                dependencies = dependencies,
                entryDependencies = entryDependencies,
            };

            return result;
        }

        public static bool TryCreateEntryData(string path,bool selectDependencies, out AddressableAssetEntryData value)
        {
            value = null;

            var entry = path.GetAddressableAssetEntryByAssetPath();
            if (entry == null) return false;

            value = CreateEntryData(entry, selectDependencies);
            
            return value != null;
        }

        public static List<AddressableAssetEntryData> GetAddressablesDependencies(string assetPath,bool recursive)
        {
            var result = new List<AddressableAssetEntryData>();
            if (string.IsNullOrEmpty(assetPath))
                return result;
            
            var dependencies = new HashSet<string>();
            
            SelectDependencies(assetPath,recursive,dependencies);
            dependencies.Remove(assetPath);

            foreach (var dependency in dependencies)
            {
                if (!TryCreateEntryData(dependency, false, out var entry))
                    continue;
                result.Add(entry);
            }

            return result;
        }



        public static void SelectDependencies(string assetPath,bool recursive,HashSet<string> assets)
        {
            if (string.IsNullOrEmpty(assetPath))
                return;

            if (!assets.Add(assetPath))
                return;
            
            var dependencies = AssetDatabase.GetDependencies(assetPath);
            var assetDependencies = AssetEditorTools.GetDependenciesFromAsset(assetPath);
            
            assets.Add(assetPath);

            foreach (var dependency in dependencies)
                SelectDependencies(dependency,recursive,assets);

            foreach (var assetDependency in assetDependencies)
            {
                if (!recursive)
                {
                    assets.Add(assetDependency);
                    continue;
                }

                SelectDependencies(assetDependency, true, assets);
            }
        }
        
        public static (string buildPath, bool isLocal) GetGroupBuildPath(AddressableAssetGroup asssetGroup) 
        {
            var schema = asssetGroup.GetSchema<BundledAssetGroupSchema>();
            var buildPath = schema == null ? string.Empty : schema.BuildPath.GetValue(asssetGroup.Settings);
            var isLocal = buildPath.IndexOf(localBuildPath,StringComparison.InvariantCultureIgnoreCase) >= 0;

            return (buildPath, isLocal);
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
                var locationData = CreateResourceLocationInfo(location);
                result.Add(locationData);
            }

            return result;
        }
        
        public static ResourceLocationData CreateResourceLocationInfo(IResourceLocation location)
        {
            var dependencyPath = location.InternalId.ToAbsoluteProjectPath().FixDirectoryPath();
            //var bundle = AssetBundle.LoadFromFile(dependencyPath);
            var isRemote = location.InternalId.IndexOf(localBuildPath, StringComparison.InvariantCultureIgnoreCase) < 0;
            var result = new ResourceLocationData()
            {
                location = location,
                internalId = location.InternalId,
                primaryKey = location.PrimaryKey,
                providerId = location.ProviderId,
                dependencies = CreateResourceLocationInfo(location.Dependencies),
                resource = location.Data as Object,
                isRemote = isRemote,
                path = dependencyPath,
            };
            return result;
        }
    }
}