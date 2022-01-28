using Sirenix.OdinInspector;

#if ODIN_INSPECTOR

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;
    using System;
    using System.IO;
    using System.Text;
    using Cysharp.Threading.Tasks;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.Extensions;
    using UnityEditor.AddressableAssets;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    
    [Serializable]
    public class AddressablesDependenciesResolver
    {
        private readonly ILifeTime _lifeTime;

        #region inspector

        public string logFilePath;
        
        [Space(4)]
        [InlineProperty]
        [HideLabel]
        public AddressableEntryTree entryTree = new AddressableEntryTree();
        
        #endregion
        
        public AddressablesDependenciesResolver(ILifeTime lifeTime,string logPath)
        {
            _lifeTime = lifeTime;
            logFilePath = logPath;
        }
        
        public async UniTask ExecuteAsync()
        {
            entryTree.Reset();
            
            var logFile = logFilePath;
            var lifeTime = _lifeTime;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var groups = settings.groups;
            var stringBuilder = new StringBuilder(1000);

            foreach (var assetGroup in groups)
            {
                stringBuilder.AppendLine($"Addressables GROUP: [{assetGroup.Name}] GUID: {assetGroup.Guid}");
                
                foreach (var assetEntry in assetGroup.entries)
                {
                    var entryData = AddressableDataTools.CreateEntryData(assetEntry);
                    entryTree.entryData.Add(entryData);
                    
                    var locations = Addressables.LoadResourceLocationsAsync(assetEntry.guid);
                    var resourceLocations = await locations.ConvertToUniTask(lifeTime);

                    stringBuilder.AppendLine($"\tENTRY: [{Path.GetFileName(assetEntry.AssetPath)}] [{assetEntry.AssetPath}] GUID: {assetEntry.guid}");
                    
                    foreach (var location in resourceLocations)
                    {
                        stringBuilder.AppendLine($"\tDependencies: ");
                        var counter = 0;
                        foreach (var dependency in location.Dependencies)
                        {
                            counter++;
                            stringBuilder.AppendLine($"\t\t {counter} : InternalId: {dependency.InternalId} | Key: {dependency.PrimaryKey} | Provider: {dependency.ProviderId} | Type: {dependency.ResourceType}");
                        }
                    }
                    
                }
            }

            var logResult = stringBuilder.ToString();
            File.WriteAllText(logFile,logResult);
            Debug.Log(logResult);
            
            stringBuilder.Clear();
        }
        
    }
}

#endif