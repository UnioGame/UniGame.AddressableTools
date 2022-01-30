using System.Collections.Generic;
using UniModules.UniGame.Core.Runtime.Rx;
using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    public interface IAddressableDataFilter
    {
        public RecycleReactiveProperty<bool>  IsActive { get; }
        
        IEnumerable<AddressableAssetEntryData> ApplyFilter(IEnumerable<AddressableAssetEntryData> source);
    }
}