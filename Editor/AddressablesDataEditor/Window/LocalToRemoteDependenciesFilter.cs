using System;
using System.Collections.Generic;
using System.Linq;
using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    [Serializable]
    public class LocalToRemoteDependenciesFilter : BaseAddressableDataFilter
    {
        protected override IEnumerable<AddressableAssetEntryData> OnFilter(IEnumerable<AddressableAssetEntryData> source)
        {
            return ShowLocalWithRemote(source);
        }

        public IEnumerable<AddressableAssetEntryData> ShowLocalWithRemote(IEnumerable<AddressableAssetEntryData> source)
        {
            var result = source
                .Where(x => !x.isRemote)
                .Where(x => x.HasDependencies)
                .Where(x => x.dependencies.Any(d => d.isRemote))
                .ToList();
            
            return result;
        }

    }
}