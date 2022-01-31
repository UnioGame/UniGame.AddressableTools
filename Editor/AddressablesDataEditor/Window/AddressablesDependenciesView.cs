using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UniModules.Editor;
using UniModules.UniCore.EditorTools.Editor;

#if ODIN_INSPECTOR

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;
    using System;
    using System.IO;
    using System.Text;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
    using UnityEditor.AddressableAssets;
    using UnityEngine;

    [Serializable]
    public class AddressablesDependenciesView
    {
        private readonly ILifeTime _lifeTime;

        #region inspector
        
        [HideInInspector]
        public string logFilePath;
        
        [Space(8)]
        [InlineProperty]
        [HideLabel]
        public AddressableEntryTree entryTree = new AddressableEntryTree();
        
        #endregion
        
        public AddressablesDependenciesView(ILifeTime lifeTime,string logPath,List<IAddressableDataFilter> filters)
        {
            _lifeTime = lifeTime;
            entryTree.Initialize(filters);
            logFilePath = logPath;
        }

        public void Reset() => entryTree.Reset();
        
        public void CollectAddressableData()
        {
            entryTree.Reset();
            
            AssetEditorTools.ShowProgress(CollectDependenciesData());
        }

        private IEnumerable<ProgressData> CollectDependenciesData()
        {
            var logFile = logFilePath;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var groups = settings.groups;
            var stringBuilder = new StringBuilder(1000);

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
                stringBuilder.AppendLine($"Addressables GROUP: [{assetGroup.Name}] GUID: {assetGroup.Guid}");
                
                foreach (var assetEntry in assetGroup.entries)
                {
                    progress.Progress = entryCounter / entriesCount;
                    progress.Content = $"Process: {assetEntry.AssetPath}";
                    yield return progress;

                    entryCounter++;
                    
                    var entryData = AddressableDataTools.CreateEntryData(assetEntry,true);
                    
                    entryTree.entryData.Add(entryData);

                    stringBuilder.AppendLine($"\t{assetEntry}");
                    stringBuilder.AppendLine($"\tDependencies: ");
                    
                    var counter = 0;
                    foreach (var location in entryData.dependencies)
                    {
                        counter++;
                        stringBuilder.AppendLine($"\t\t{counter} : {location}");
                    }
                    counter = 0;
                    
                    foreach (var location in entryData.entryDependencies)
                    {
                        counter++;
                        stringBuilder.AppendLine($"\t\t{counter} : {location}");
                    }
                }
                
                
            }

            var logResult = stringBuilder.ToString();
            File.WriteAllText(logFile,logResult);
            Debug.Log(logResult);
            
            stringBuilder.Clear();

            progress.IsDone = true;
            
            entryTree.Refresh();
            
            yield return progress;
        }

    }
}

#endif