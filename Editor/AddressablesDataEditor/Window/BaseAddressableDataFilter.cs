#if ODIN_INSPECTOR

namespace UniGame.AddressableTools.Editor
{
    using System;
    using System.Collections.Generic;
    using global::UniGame.AddressableTools.Editor;

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