using System.Linq;
using UniModules.UniCore.Runtime.DataFlow;
using UniModules.UniCore.Runtime.Rx.Extensions;
using UniRx;
using UnityEngine;

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using System;
    using System.Collections.Generic;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;

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

        public void UpdateFilters(string filter,IEnumerable<IAddressableDataFilter> filters)
        {
            filteredData.Clear();

            var result = ApplyFilters(entryData,filters);
            
            if (string.IsNullOrEmpty(filter))
            {
                filteredData.AddRange(result);
            }
            else
            {
                filteredData.AddRange(result.Where(x => x.IsMatch(filter)));
            }

        }

        public IEnumerable<AddressableAssetEntryData> ApplyFilters(IEnumerable<AddressableAssetEntryData> source,IEnumerable<IAddressableDataFilter> filters)
        {
            foreach (var filter in filters)
                source = filter.ApplyFilter(source);
            return source;
        }
        

        public void Reset()
        {
            entryData.Clear();
        }
        
    }
}