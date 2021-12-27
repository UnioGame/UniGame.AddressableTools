﻿using System;
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
        public AddressableAssetEntryData entry;
        
        public AddressableAssetView Initialize(Object target)
        {
            if (target == null)
            {
                Reset();
                return this;
            }
            
            asset = target;

            if (target == null) return this;
            
            var assetEntry = target.GetAddressableAssetEntry();
            entry = AddressableDataTools.CreateEntryData(assetEntry);
            return this;
        }
        
        public void Reset()
        {
            asset = null;
            entry = null;
        }
        
        public void Refresh()
        {
            Initialize(asset);
        }
        
    }
}