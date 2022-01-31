#if ODIN_INSPECTOR

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using System;
    using System.Collections.Generic;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;

    [Serializable]
    public class BaseAddressableDataFilter : IAddressableDataFilter
    {
        public bool isEnabled;
        
        public IEnumerable<AddressableAssetEntryData> ApplyFilter(IEnumerable<AddressableAssetEntryData> source)
        {
            return !isEnabled ? source : OnFilter(source);
        }

        protected virtual IEnumerable<AddressableAssetEntryData> OnFilter(IEnumerable<AddressableAssetEntryData> source)
        {
            return source;
        }
    }
}

#endif