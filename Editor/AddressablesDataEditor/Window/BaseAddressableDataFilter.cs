using Sirenix.OdinInspector;
using UnityEngine;

#if ODIN_INSPECTOR

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using System;
    using System.Collections.Generic;
    using UniModules.UniGame.Core.Runtime.Rx;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;

    [Serializable]
    public class BaseAddressableDataFilter : IAddressableDataFilter
    {
        [OnValueChanged(nameof(UpdateStatus))]
        [InlineButton(nameof(Refresh),nameof(Refresh))]
        public bool isEnabled;
        
        [HideInInspector]
        public RecycleReactiveProperty<bool> isActive = new RecycleReactiveProperty<bool>(false);

        public virtual RecycleReactiveProperty<bool>  IsActive => isActive;

        public void UpdateStatus()
        {
            isActive.Value = isEnabled;
        }

        public void Refresh()
        {
            isActive.SetValueForce(isEnabled);
        }
        
        public IEnumerable<AddressableAssetEntryData> ApplyFilter(IEnumerable<AddressableAssetEntryData> source)
        {
            return !IsActive.Value ? source : OnFilter(source);
        }

        protected virtual IEnumerable<AddressableAssetEntryData> OnFilter(IEnumerable<AddressableAssetEntryData> source)
        {
            return source;
        }
    }
}

#endif