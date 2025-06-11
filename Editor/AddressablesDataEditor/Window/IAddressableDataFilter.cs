namespace UniGame.AddressableTools.Editor
{
    using System.Collections.Generic;
    using global::UniGame.AddressableTools.Editor;

    public interface IAddressableDataFilter
    {
        IEnumerable<AddressableAssetEntryData> ApplyFilter(IEnumerable<AddressableAssetEntryData> source);
    }
}