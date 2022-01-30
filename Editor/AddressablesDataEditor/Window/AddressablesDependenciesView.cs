using System.Collections.Generic;
using Sirenix.OdinInspector;

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
            
            var logFile = logFilePath;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var groups = settings.groups;
            var stringBuilder = new StringBuilder(1000);

            foreach (var assetGroup in groups)
            {
                stringBuilder.AppendLine($"Addressables GROUP: [{assetGroup.Name}] GUID: {assetGroup.Guid}");
                
                foreach (var assetEntry in assetGroup.entries)
                {
                    var entryData = AddressableDataTools.CreateEntryData(assetEntry,true);
                    
                    entryTree.entryData.Add(entryData);

                    stringBuilder.AppendLine($"\t{assetEntry}");
                    stringBuilder.AppendLine($"\tDependencies: ");
                    
                    foreach (var location in entryData.dependencies)
                    {
                        var counter = 0;
                        counter++;
                        stringBuilder.AppendLine($"\t\t{counter} : {location}");
                    }
                }
                
                entryTree.Refresh();
            }

            var logResult = stringBuilder.ToString();
            File.WriteAllText(logFile,logResult);
            Debug.Log(logResult);
            
            stringBuilder.Clear();
        }
        
    }
}

#endif