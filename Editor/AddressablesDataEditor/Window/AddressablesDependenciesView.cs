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
        [InlineButton(nameof(Search),label:nameof(Search))]
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

        public void Search() => entryTree.UpdateFilters(search,_configuration.filters);
        
        public void Reset() => entryTree.Reset();
        
        public void CollectAddressableData()
        {
            entryTree.Reset();
            
            AssetEditorTools.ShowProgress(CollectDependenciesData());
        }

        private IEnumerable<ProgressData> CollectDependenciesData()
        {
            var logFile = _configuration.LogPath;
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
            if (entriesCount == 0)
               yield break;

            var entryCounter = 0f;
            
            foreach (var assetGroup in groups)
            {
                _stringBuilder.AppendLine($"Addressables GROUP: [{assetGroup.Name}] GUID: {assetGroup.Guid}");
                
                foreach (var assetEntry in assetGroup.entries)
                {
                    progress.Progress = entryCounter / entriesCount;
                    progress.Content = $"Process: {assetEntry.AssetPath}";
                    yield return progress;

                    entryCounter++;
                    
                    var entryData = AddressableDataTools.CreateEntryData(assetEntry,true,_configuration.collectAddressablesRecursive);
                    
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

            Search();
            
            var logResult = _stringBuilder.ToString();
            File.WriteAllText(logFile,logResult);
            Debug.Log(logResult);
            
            _stringBuilder.Clear();

            progress.IsDone = true;

            yield return progress;
        }

    }
}

#endif