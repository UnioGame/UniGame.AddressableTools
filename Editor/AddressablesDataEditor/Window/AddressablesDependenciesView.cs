#if ODIN_INSPECTOR

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using UniModules.Editor;
    using UniModules.UniCore.EditorTools.Editor;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;
    using System;
    using System.IO;
    using System.Text;
    using UnityEditor.AddressableAssets;
    using UnityEngine;

    [Serializable]
    public class AddressablesDependenciesView
    {
        private readonly AddressablesDependenciesConfiguration _configuration;
        private readonly StringBuilder _stringBuilder = new StringBuilder(1000);
        
        #region inspector
        
#if ODIN_INSPECTOR
        //[OnValueChanged(nameof(Search))]
#endif        
        public string search;

        [Space(8)]
        [InlineProperty]
        [HideLabel]
        public AddressableEntryTree entryTree = new AddressableEntryTree();
        
        #endregion
        
        public AddressablesDependenciesView(AddressablesDependenciesConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Button]
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        public void Find()
        {
            var filters = _configuration.filters;
            entryTree.UpdateFilters(search,filters);
        }

        [Button]
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        public void ResetFilter() => entryTree.ResetFilter();
        
        [Button]
        [ResponsiveButtonGroup()]
        [PropertyOrder(-1)]
        public void Print() => Print(entryTree.entryData);

        [Button]
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        public void PrintFiltered() => Print(entryTree.filteredData);

        
        public void Reset() => entryTree.Reset();

        public void Print(List<AddressableAssetEntryData> entryDatas)
        {
            _stringBuilder.Clear();
            var entryCounter = 0f;
            var groups = entryDatas.GroupBy(x => x.groupName);
            
            foreach (var assetGroup in groups)
            {
                _stringBuilder.AppendLine($"Addressables GROUP: [{assetGroup.Key}]");

                foreach (var assetEntry in assetGroup)
                {
                    entryCounter++;
                    
                    var entryData = assetEntry;
                    
                    entryTree.entryData.Add(entryData);

                    _stringBuilder.AppendLine($"\t{assetEntry}");
                    _stringBuilder.AppendLine($"\tDependencies: ");
                    
                    var counter = 0;
                    foreach (var location in entryData.dependencies)
                    {
                        counter++;
                        _stringBuilder.AppendLine($"\t\t{counter} : {location}");
                    }
                    counter = 0;
                    
                    foreach (var location in entryData.entryDependencies)
                    {
                        counter++;
                        _stringBuilder.AppendLine($"\t\t{counter} : {location}");
                    }
                }
            }

            var logResult = _stringBuilder.ToString();
            File.WriteAllText(_configuration.LogPath,logResult);
            Debug.Log(logResult);
            
            _stringBuilder.Clear();
        }
        
        public void CollectAddressableData()
        {
            try
            {
                Reset();
                CollectDependenciesData();
            }
            finally
            {
                AssetEditorTools.ClearProgress();
            }
        }

        private void CollectDependenciesData()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var groups = settings.groups;
            
            var progress = new ProgressData()
            {
                Content = "Initialize..",
                Progress = 0,
                Title = "Collecting Dependencies...",
                IsDone = false,
            };

            var entriesCount =  groups.Sum(x => x.entries.Count);
            if (entriesCount <= 0)
                return;

            var entryCounter = 0f;

            foreach (var assetGroup in groups)
            {
                foreach (var assetEntry in assetGroup.entries)
                {
                    progress.Progress = entryCounter / entriesCount;
                    progress.Content = $"Process: {assetEntry.AssetPath}";
                    
                    AssetEditorTools.ShowProgress(progress);

                    entryCounter++;
                    
                    var entryData = AddressableDataTools.CreateEntryData(assetEntry,true,_configuration.collectAddressablesRecursive);
                    entryTree.entryData.Add(entryData);
                }
                
            }

            ResetFilter();

            progress.IsDone = true;
            progress.Progress = 1;
            AssetEditorTools.ShowProgress(progress);
        }

    }
}

#endif