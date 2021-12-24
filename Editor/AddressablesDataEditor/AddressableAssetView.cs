using System;
using Sirenix.OdinInspector;
using UniModules.UniGame.AddressableExtensions.Editor;
using UnityEditor.AddressableAssets.Settings;
using Object = UnityEngine.Object;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableDataEditor
{
    [Serializable]
    public class AddressableAssetView
    {
        [InlineEditor]
        [OnValueChanged(nameof(Refresh))]
        public Object asset;

        [InlineProperty]
        [HideLabel]
        public AddressableAssetEntry entry;

        [InlineProperty]
        [HideLabel]
        public AddressableAssetGroup group;

        public AddressableAssetView Initialize(Object target)
        {
            if (target == null)
            {
                Reset();
                return this;
            }
            
            asset = target;

            if (target == null) return this;
            
            entry = target.GetAddressableAssetEntry();
            group = entry.parentGroup;
            return this;
        }
        
        public void Reset()
        {
            asset = null;
            entry = null;
            group = null;
        }
        
        public void Refresh()
        {
            Initialize(asset);
        }
        
    }
}