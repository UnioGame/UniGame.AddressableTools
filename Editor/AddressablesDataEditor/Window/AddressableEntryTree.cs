namespace UniGame.AddressableTools.Editor
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    using System;
    using System.Collections.Generic;
    using global::UniGame.AddressableTools.Editor;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    public class AddressableEntryTree
    {
        
#if ODIN_INSPECTOR
        [HideInInspector]
#endif
        public List<AddressableAssetEntryData> entryData = new List<AddressableAssetEntryData>();

#if ODIN_INSPECTOR
        [TableList(ShowPaging = true,HideToolbar = false,DrawScrollView = true,ShowIndexLabels = true)]
#endif
        public List<AddressableAssetEntryData> filteredData = new List<AddressableAssetEntryData>();

        public void ResetFilter()
        {
            filteredData.Clear();
            filteredData.AddRange(entryData);
        }
        
        public void UpdateFilters(string filter,List<IAddressableDataFilter> filters)
        {
            filteredData = new List<AddressableAssetEntryData>();

            var result = ApplyFilters(entryData,filters).ToList();
            
            if (string.IsNullOrEmpty(filter))
            {
                filteredData.AddRange(result);
            }
            else
            {
                foreach (var data in result) 
                {
                    if(data.IsMatch(filter)) filteredData.Add(data);
                }
            }
        }

        public IEnumerable<AddressableAssetEntryData> ApplyFilters(IEnumerable<AddressableAssetEntryData> source,IEnumerable<IAddressableDataFilter> filters)
        {
            try
            {
                foreach (var filter in filters)
                {
                    var isCanceled = EditorUtility.DisplayCancelableProgressBar($"Apply Filters",$"filter {filter.GetType().Name}",0);
                    if (isCanceled) break;
                    source = filter.ApplyFilter(source);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return source;
        }
        

        public void Reset()
        {
            entryData.Clear();
            filteredData.Clear();
        }
        
    }
}