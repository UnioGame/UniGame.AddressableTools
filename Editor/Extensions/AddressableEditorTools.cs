using System;
using System.IO;
using System.Reflection;
using UniModules.UniCore.Runtime.ReflectionUtils;
using UniModules.UniCore.Runtime.Utils;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace UniModules.UniGame.AddressableExtensions.Editor
{
    using System.Collections;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using System.Collections.Generic;
    using System.Linq;
    using UniModules.Editor;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public static class AddressableEditorTools
    {
                
        private static List<IResourceLocator> _resourceLocators = new List<IResourceLocator>();
        private static AddressablesResourceComparer addressablesComparerInstance = new AddressablesResourceComparer();
        private static AddressableAssetSettings addressableAssetSettings;

        private static MemorizeItem<ResourceLocatorType, IResourceLocator> resourceLocators =
            MemorizeTool.Memorize<ResourceLocatorType, IResourceLocator>(CreateLocator);

        public static AssetReference FindReferenceByAddress(this AddressableAssetSettings settings,string address)
        {
            var entries = new List<AddressableAssetEntry>();
            settings.GetAllAssets(entries,true,null,x => x.address == address);
            var asset = entries.FirstOrDefault();
            
            if (asset == null) {
                Debug.LogWarning($"Not found asset with address :: {address}");
                return null;
            }
            return new AssetReference(asset.guid);
        }

        public static IResourceLocator CreateLocator(ResourceLocatorType locatorType)
        {
            return locatorType switch
            {
                ResourceLocatorType.AddressableSettingsLocator => CreateAressableSettingsLocator(),
                ResourceLocatorType.ResourceLocationMap => CreateResourceLocatorMap(),
                _ => default
            };
        }


        public static IResourceLocator CreateResourceLocatorMap()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
                return new DummyLocator();
            
            var settingsPath = Addressables.BuildPath + "/settings.json";
            if (!File.Exists(settingsPath))
            {
                Debug.LogError("Player content must be built before entering play mode with packed data.  This can be done from the Addressables window in the Build->Build Player Content menu command.");
                return new DummyLocator();
            }
            
            var rtd = JsonUtility.FromJson<ResourceManagerRuntimeData>(File.ReadAllText(settingsPath));

            var locationMap = new ResourceLocationMap("CatalogLocator", rtd.CatalogLocations);
            return locationMap;
        }

        public static IResourceLocator CreateAressableSettingsLocator()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
                return new DummyLocator();
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var type = ReflectionTools.GetTypeByName("AddressableAssetSettingsLocator");
            var constructors = type.GetConstructors();
            var constructor = constructors.First();
            var locator = constructor.Invoke(new object[] { settings }) as IResourceLocator;
            return locator;
        }
        
        public static object EvaluateKey(object obj)
        {
            return obj is IKeyEvaluator ? (obj as IKeyEvaluator).RuntimeKey : obj;
        }
        
        
        public static List<IResourceLocation> GatherDependenciesFromLocations(IList<IResourceLocation> locations)
        {
            var locHash = new HashSet<IResourceLocation>();
            foreach (var loc in locations)
            {
                if (loc.ResourceType == typeof(IAssetBundleResource))
                {
                    locHash.Add(loc);
                }
                if (loc.HasDependencies)
                {
                    foreach (var dep in loc.Dependencies)
                        if (dep.ResourceType == typeof(IAssetBundleResource))
                            locHash.Add(dep);
                }
            }
            return new List<IResourceLocation>(locHash);
        }

        public static IList<IResourceLocation> GetAddressableResourceDependencies(object key, Type type)
        {
            IList<IResourceLocation> locations;
            if (!GetResourceLocations(key, type, out locations))
                return locations;
            
            var dlLocations = GatherDependenciesFromLocations(locations);
            return dlLocations;
        }

        public static bool GetResourceLocations(object key, Type type, out IList<IResourceLocation> locations)
        {
            locations = null;
            
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
                return false;
            
            if (type == null && key is AssetReference reference)
                type = reference.editorAsset.GetType();
            
            key = EvaluateKey(key);
            
            locations = null;
            HashSet<IResourceLocation> current = null;

            _resourceLocators.Clear();
            _resourceLocators.Add(CreateLocator(ResourceLocatorType.ResourceLocationMap));

            foreach (var locator in _resourceLocators)
            {
                IList<IResourceLocation> locs;
                if (!locator.Locate(key, type, out locs)) 
                    continue;
                
                if (locations == null)
                {
                    //simple, common case, no allocations
                    locations = locs;
                    continue;
                }

                //less common, need to merge...
                if (current == null)
                {
                    current = new HashSet<IResourceLocation>();
                    foreach (var loc in locations)
                        current.Add(loc);
                }

                current.UnionWith(locs);
            }

            if (current == null)
                return locations != null;

            locations = new List<IResourceLocation>(current);
            
            return true;
        }

        public static bool GetResourceLocations(IEnumerable keys, Type type, Addressables.MergeMode merge, out IList<IResourceLocation> locations)
        {
            locations = null;
            HashSet<IResourceLocation> current = null;
            foreach (var key in keys)
            {
                IList<IResourceLocation> locs;
                if (GetResourceLocations(key, type, out locs))
                {
                    if (locations == null)
                    {
                        locations = locs;
                        if (merge == Addressables.MergeMode.UseFirst)
                            return true;
                    }
                    else
                    {
                        current ??= new HashSet<IResourceLocation>(locations, addressablesComparerInstance);

                        switch (merge)
                        {
                            case Addressables.MergeMode.Intersection:
                                current.IntersectWith(locs);
                                break;
                            case Addressables.MergeMode.Union:
                                current.UnionWith(locs);
                                break;
                        }
                    }
                }
                else
                {
                    //if entries for a key are not found, the intersection is empty
                    if (merge == Addressables.MergeMode.Intersection)
                    {
                        locations = null;
                        return false;
                    }
                }
            }

            if (current == null)
                return locations != null;
            if (current.Count == 0)
            {
                locations = null;
                return false;
            }
            locations = new List<IResourceLocation>(current);
            return true;
        }
        
        
        public static AssetReference FindReferenceByAddress(string address)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            return FindReferenceByAddress(settings, address);
        }
        
        public static string EvaluateActiveProfileString(this string key)
        {
            addressableAssetSettings = addressableAssetSettings ? addressableAssetSettings :  AssetEditorTools.GetAsset<AddressableAssetSettings>();
            if (!addressableAssetSettings) return key;
            var activeprofile = addressableAssetSettings.activeProfileId;
            var result = addressableAssetSettings.profileSettings.EvaluateString(activeprofile, key);
            return result;
        }
        
        public static AddressableAssetEntry CreateAssetEntry<T>(T source, string groupName, string label) where T : UnityEngine.Object
        {
            var entry = CreateAssetEntry(source, groupName);
            if (source != null) {
                source.AddAddressableAssetLabel(label);
            }

            return entry;
        }

        public static AddressableAssetEntry CreateAssetEntry<T>(T source, string groupName) where T : Object
        {
            if (source == null || string.IsNullOrEmpty(groupName) || !AssetDatabase.Contains(source))
                return null;
            
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            var sourcePath = AssetDatabase.GetAssetPath(source);
            var sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath);
            var group = !GroupExists(groupName) ? CreateGroup(groupName) : GetGroup(groupName);

            var entry = addressableSettings.CreateOrMoveEntry(sourceGuid, group);
            entry.address = sourcePath;
            
            addressableSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);

            return entry;
        }

        public static AddressableAssetEntry MarkDirty(this AddressableAssetEntry entry)
        {
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            addressableSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);
            return entry;
        }

        public static AddressableAssetEntry CreateAssetEntry<T>(T source) where T : Object
        {
            if (source == null || !AssetDatabase.Contains(source))
                return null;
            
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            var sourcePath = AssetDatabase.GetAssetPath(source);
            var sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath);
            var entry = addressableSettings.CreateOrMoveEntry(sourceGuid, addressableSettings.DefaultGroup);
            entry.address = sourcePath;
            
            addressableSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);

            return entry;
        }

        public static AddressableAssetGroup GetGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return null;
            
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            return addressableSettings.FindGroup(groupName);
        }

        public static AddressableAssetGroup CreateGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return null;
            
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            var group = addressableSettings.CreateGroup(groupName, false, false, false, addressableSettings.DefaultGroup.Schemas);
            
            addressableSettings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded, group, true);

            return group;
        }

        public static bool GroupExists(string groupName)
        {
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            return addressableSettings.FindGroup(groupName) != null;
        }
    }
    
}