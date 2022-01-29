#if ODIN_INSPECTOR

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor.Window
{
    using System;
    using Sirenix.OdinInspector;

    [Serializable]
    public class AddressableGroupData : ISearchFilterable
    {
        public string guid;
        public string groupName;
        public string buildPath;
        public bool isRemote;
        
        public override int GetHashCode() => guid == null ? 0 : guid.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is AddressableAssetEntryData entryData)
                return entryData.guid == guid;
            return base.Equals(obj);
        }
        
        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return true;
            
            var result = !string.IsNullOrEmpty(guid) && guid.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(groupName) && groupName.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(buildPath) && buildPath.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            //result |= labels.Any(x => x.IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase) >= 0);
            result |= isRemote && nameof(isRemote).IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase)>=0;

            return result;
        }
    }
}

#endif
