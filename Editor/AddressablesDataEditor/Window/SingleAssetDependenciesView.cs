using System.Linq;
using UnityEditor;

#if ODIN_INSPECTOR

namespace UniGame.AddressableTools.Editor.Window
{
    using System;
    using Sirenix.OdinInspector;
    using global::UniGame.AddressableTools.Editor;

    [Serializable]
    public class SingleAssetDependenciesView
    {
        [OnValueChanged(nameof(UpdateGuid))]
        [PropertyOrder(-1)]
        public string guid;
        
        [PropertyOrder(-1)]
        [OnValueChanged(nameof(Update))]
        [InlineEditor()]
        public UnityEngine.Object asset;

        [OnValueChanged(nameof(Update))]
        public bool selectDependencies = true;

        [OnValueChanged(nameof(Update))]
        public bool recursiveDependencies = false;

        [OnValueChanged(nameof(Update))]
        public bool showRemoteDependencies = false;
        
        [TitleGroup(nameof(entry))]
        [InlineProperty]
        [HideLabel]
        public AddressableAssetEntryData entry;

        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        [Button]
        public void Reset()
        {
            entry = new AddressableAssetEntryData();
            asset = null;
        }
        
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        [Button]
        public void Update()
        {
            if (asset == null)
            { 
                return;
            }

            var assetEntry = asset.GetAddressableAssetEntry();
            if (assetEntry == null) return;
                
            entry = AddressableDataTools.CreateEntryData(assetEntry,selectDependencies,recursiveDependencies);
            
            if (showRemoteDependencies)
            {
                entry.dependencies = entry.dependencies.Where(x => x.isRemote).ToList();
                entry.entryDependencies = entry.entryDependencies.Where(x => x.isRemote).ToList();
            }
        }

        public void UpdateGuid()
        {
            if(string.IsNullOrEmpty(guid))
                return;
            
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var targetAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (!targetAsset) return;

            asset = targetAsset;
            Update();
        }

    }
}

#endif