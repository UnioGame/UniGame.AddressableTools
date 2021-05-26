using System.Collections.Generic;
using System.Text;
using UniModules.UniCore.EditorTools.Editor.Utility;
using UniModules.UniGame.AddressableExtensions.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UniModules.UniBuild.Commands
{
    
    public struct AddressableAssetEntryError
    {
        public AddressableAssetEntry Entry;
        public string                Error;
    }
    
    public static class AddressablesAssetsFix
    {
        [MenuItem(itemName: "UniGame/Addressables/Validate Addressables Guid's")]
        public static void Validate()
        {
            var status = ValidateAddressablesGuid();
            PrintStatus(status.isValid,status.errors,LogType.Error);
        }

        [MenuItem(itemName: "UniGame/Addressables/Fix Addressables Guid's")]
        public static void FixAddressablesGuids()
        {
            var status = ValidateAddressablesGuid();
            PrintStatus(status.isValid,status.errors,LogType.Warning);
            FixAddressablesGuids(status.errors);
        }

        public static void FixAddressablesGuids(List<AddressableAssetEntryError> errors)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings) return;
        
            foreach (var assetEntryError in errors)
            {
                var entry      = assetEntryError.Entry;
                settings.RemoveAssetEntry(entry.guid);
            }
        
            settings.MarkDirty();
            AssetDatabase.Refresh();
        
            foreach (var assetEntryError in errors)
            {
                var entry             = assetEntryError.Entry;
                var asset             = AssetDatabase.LoadAssetAtPath<Object>(entry.AssetPath);
                var assetGroup        = entry.parentGroup;
            
                asset.SetAddressableAssetGroup(assetGroup);
                var assetEntry = asset.GetAddressableAssetEntry();
                Debug.Log($"create addressable entry {assetEntry?.parentGroup.name} : {assetEntry?.guid} {assetEntry?.AssetPath} ");
            }
        
            settings.MarkDirty();
            AssetDatabase.Refresh();
        }
    
        public static void PrintStatus(bool isValid,List<AddressableAssetEntryError> errors,LogType logType)
        {
            if (isValid)
            {
                Debug.Log($"Addressables GUID Validated");
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Addressables GUID Validator find errors [{errors.Count}]:");
            foreach (var error in errors)
            {
                builder.AppendLine(error.Error);
            }

            var errorMessage = builder.ToString();
        
            switch (logType)
            {
                case LogType.Error:
                    Debug.LogError(errorMessage);
                    break;
                case LogType.Assert:
                    Debug.LogAssertion(errorMessage);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(errorMessage);
                    break;
                case LogType.Log:
                    Debug.Log(errorMessage);
                    break;
                default:
                    Debug.Log(errorMessage);
                    break;
            }
        }
    
        public static (bool isValid,List<AddressableAssetEntryError> errors) ValidateAddressablesGuid()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var errors   = new List<AddressableAssetEntryError>();
        
            if (!settings) return (true,errors);

            var groups = settings.groups;
            foreach (var addressableAssetGroup in groups)
            {
                var entries = addressableAssetGroup.entries;
                foreach (var entry in entries)
                {
                    var entryStatus = Validate(entry);
                    if(!string.IsNullOrEmpty(entryStatus.Error))
                        errors.Add(entryStatus);
                }
            }

            return (errors.Count <= 0, errors);
        }

        public static AddressableAssetEntryError Validate(AddressableAssetEntry entry)
        {
            var assetByPath   = AssetDatabase.LoadAssetAtPath<Object>(entry.AssetPath);
            var assetGuidPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            var assetByGuid   = AssetDatabase.LoadAssetAtPath<Object>(assetGuidPath);
            var entryParentGroup         = entry.parentGroup;

            var isValid = assetByGuid == assetByPath;
            if (isValid) return new AddressableAssetEntryError();

            var errorMessage =
                $"ERROR ADDRESSABLE REF AT GROUP : {entryParentGroup.Name} : {entry.address} \n\t {assetByGuid?.name}: {entry.guid}  by GUID != \n\t {assetByPath?.name} : {entry.AssetPath} by PATH ";

            return new AddressableAssetEntryError()
            {
                Entry = entry,
                Error = errorMessage
            };
        }
    
    }
}