using System;
using System.Collections.Generic;
using System.Linq;
using UniGame.AddressableTools.Editor;

namespace UniGame.AddressableTools.Editor
{
    [Serializable]
    public class LocalToRemoteDependenciesFilter : BaseAddressableDataFilter
    {
        public bool checkAddressableDependencies = true;
        public bool checkLocationDependencies = false;
    
        protected override IEnumerable<AddressableAssetEntryData> OnFilter(IEnumerable<AddressableAssetEntryData> source)
        {
            return ShowLocalWithRemote(source);
        }

        public IEnumerable<AddressableAssetEntryData> ShowLocalWithRemote(IEnumerable<AddressableAssetEntryData> source)
        {
            if (checkAddressableDependencies)
            {
                source = source.Where(x => !x.isRemote)
                    .Where(x => x.entryDependencies.Any(d => d.isRemote))
                    .ToList();
            }

            if (checkLocationDependencies)
            {
                source = source.Where(x => !x.isRemote)
                    .Where(x => x.dependencies.Any(d => d.isRemote))
                    .ToList();
            }

            return source;
        }
    }

}