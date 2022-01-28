using System.Linq;

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
        public Object asset;
        public string address;
        public string groupName;
        public bool readOnly;
        public List<string> labels = new List<string>();

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
            result |= !string.IsNullOrEmpty(address) && address.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(groupName) && groupName.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= labels.Any(x => x.IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase) >= 0);
            result |= isRemote && nameof(isRemote).IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase)>=0;

            return result;
        }

    }
}