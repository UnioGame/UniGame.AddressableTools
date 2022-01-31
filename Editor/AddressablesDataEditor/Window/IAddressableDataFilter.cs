namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using System.Collections.Generic;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;

    public interface IAddressableDataFilter
    {
        IEnumerable<AddressableAssetEntryData> ApplyFilter(IEnumerable<AddressableAssetEntryData> source);
    }
}