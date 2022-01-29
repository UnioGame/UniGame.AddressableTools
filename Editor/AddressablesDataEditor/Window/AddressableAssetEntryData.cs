using System.IO;
using System.Linq;
using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor.Window;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    using System;
    using System.Collections.Generic;
    using Object = UnityEngine.Object;
    
    [Serializable]
    public class AddressableAssetEntryData
#if ODIN_INSPECTOR
        : ISearchFilterable
#endif
    {
        public string guid;
        public bool isRemote;
        public string address;
        public string groupName;
        public string buildPath;
        public bool readOnly;
        public List<string> labels = new List<string>();

#if ODIN_INSPECTOR
        [InlineEditor]
        [HideIf(nameof(IsNullAsset))]
#endif
        public Object asset;
        
#if ODIN_INSPECTOR
        [FoldoutGroup(nameof(dependencies))]
        [ShowIf(nameof(HasDependencies))]
#endif
        public List<AddressableGroupData> dependencies = new List<AddressableGroupData>();
#if ODIN_INSPECTOR
        [FoldoutGroup(nameof(dependencies))]
        [ShowIf(nameof(HasDependenciesLocations))]
#endif
        public List<ResourceLocationData> dependenciesLocations = new List<ResourceLocationData>();

        public bool IsNullAsset => asset == null;
        
        public bool HasDependenciesLocations => dependenciesLocations.Count > 0;
        public bool HasDependencies => dependencies.Count > 0;
        
        public override int GetHashCode() => guid == null ? 0 : guid.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is AddressableAssetEntryData entryData)
                return entryData.guid == guid;
            return base.Equals(obj);
        }

        public override string ToString()
        {
            var assetName = asset == null ? string.Empty : asset.name;
            return $"ENTRY: GUID: {guid} | NAME: [{assetName}] [{address}] | GROUP: {groupName} | IS_REMOTE: {isRemote}";
        }

        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return true;
            
            var result = !string.IsNullOrEmpty(guid) && guid.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(address) && address.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(groupName) && groupName.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(buildPath) && buildPath.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= labels.Any(x => x.IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase) >= 0);
            result |= isRemote && nameof(isRemote).IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase)>=0;
            result |= dependencies.Any(x => x.IsMatch(searchString));
            result |= dependenciesLocations.Any(x => x.IsMatch(searchString));

            return result;
        }

    }
}