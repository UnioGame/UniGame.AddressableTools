#if ODIN_INSPECTOR


namespace UniGame.AddressableTools.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::UniGame.AddressableTools.Editor;

    
    [Serializable]
    public class EntryDependenciesFilter : BaseAddressableDataFilter
    {
        public bool filterWithEntryDependencies = true;
        public bool filterWithLocationDependencies = false;

        protected override IEnumerable<AddressableAssetEntryData> OnFilter(IEnumerable<AddressableAssetEntryData> source)
        {
            if (filterWithEntryDependencies)
                source = source.Where(x => x.entryDependencies.Count > 0);
            if (filterWithLocationDependencies)
                source = source.Where(x => x.dependencies.Count > 0);

            return source;
        }
    }
}

#endif