﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor
{
    public static class AddressableDataTools
    {
        public const string localBuildPath = "com.unity.addressables";
        
        public static AddressableAssetEntryData CreateEntryData(AddressableAssetEntry entry)
        {
            if (entry == null) return new AddressableAssetEntryData();
            
            var parent = entry.parentGroup;
            var settings = parent.Settings;
            
            var schema = parent.GetSchema<BundledAssetGroupSchema>();
            string buildPath = schema.BuildPath.GetValue(parent.Settings);
            
            //var baseBuildPathValue = settings.profileSettings.GetValueById(settings.activeProfileId, m_BuildPath.Id);
            //var remotePath = settings.profileSettings.EvaluateString(settings.activeProfileId, baseBuildPathValue);

            var isRemote = false;// buildPath.IndexOf(remoteBuildPath,StringComparison.InvariantCultureIgnoreCase) >= 0;
            
            var result = new AddressableAssetEntryData()
            {
                guid = entry.guid,
                isRemote = isRemote,
                address = entry.address,
                labels = entry.labels.ToList(),
                groupName = entry.parentGroup.Name,
                readOnly = entry.ReadOnly,
                asset = entry.MainAsset,
            };

            return result;
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
                id = location.InternalId,
                key = location.PrimaryKey,
                providerId = location.ProviderId,
                dependencies = CreateResourceLocationInfo(location.Dependencies),
                resourceType = location.ResourceType
            };
            return result;
        }
    }
}