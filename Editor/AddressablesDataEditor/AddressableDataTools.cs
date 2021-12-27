using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableDataEditor
{
    public static class AddressableDataTools
    {

        public static AddressableAssetEntryData CreateEntryData(AddressableAssetEntry entry)
        {
            if (entry == null) return new AddressableAssetEntryData();
            
            var result = new AddressableAssetEntryData()
            {
                guid = entry.guid,
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