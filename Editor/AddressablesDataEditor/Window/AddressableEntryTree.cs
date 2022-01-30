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
        [OnValueChanged(nameof(OnFilterChanged))]
        [InlineButton(nameof(Refresh),label:nameof(Refresh))]
#endif        
        public string search;

        [OnValueChanged(nameof(OnInitialize))]
        [SerializeReference]
        [InlineProperty]
        [ListDrawerSettings()]
        public List<IAddressableDataFilter> filters = new List<IAddressableDataFilter>()
        {
            new LocalToRemoteDependenciesFilter(),
        };
        
#if ODIN_INSPECTOR
        [HideInInspector]
#endif
        public List<AddressableAssetEntryData> entryData = new List<AddressableAssetEntryData>();

#if ODIN_INSPECTOR
        [TableList]
#endif
        public List<AddressableAssetEntryData> filteredData = new List<AddressableAssetEntryData>();

        private LifeTimeDefinition _lifeTime = new LifeTimeDefinition();

        public void Initialize(List<IAddressableDataFilter> filtersData)
        {
            filters = filtersData;
            OnInitialize();
        }
        
        public void Refresh() =>  OnFilterChanged(search);
        
        public void OnFilterChanged(string filter)
        {
            filteredData.Clear();

            var result = ApplyFilters(entryData);
            
            if (string.IsNullOrEmpty(filter))
            {
                filteredData.AddRange(result);
            }
            else
            {
                filteredData.AddRange(result.Where(x => x.IsMatch(filter)));
            }

        }

        public IEnumerable<AddressableAssetEntryData> ApplyFilters(IEnumerable<AddressableAssetEntryData> source)
        {
            foreach (var filter in filters)
                source = filter.ApplyFilter(source);
            return source;
        }
        

        public void Reset()
        {
            entryData.Clear();
        }

        private void OnFiltersChanged()
        {
            foreach (var filter in filters)
            {
                filter.IsActive.Subscribe(x => Refresh())
                    .AddTo(_lifeTime);
            }
        }

        [OnInspectorInit]
        private void OnInitialize()
        {
            _lifeTime?.Release();
            _lifeTime = new LifeTimeDefinition();
            OnFiltersChanged();
        }
        
    }
}