namespace UniGame.AddressableAtlases
{
    using UnityEngine.U2D;
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
    using UniModules.UniGame.AddressableExtensions.Editor;
    using UniModules.Editor;
#endif
    
    [Serializable]
    [CreateAssetMenu(menuName = "UniGame/Services/AddressableAtlases/AddressableAtlases Settings",
        fileName = "AddressableAtlases Settings")]
    public class AddressableAtlasesSettingsAsset : ScriptableObject
    {
        #region inspector

        [HideLabel]
        [InlineProperty]
        public AddressableAtlasSettings settings = new();

        #endregion
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void Reimport()
        {
#if UNITY_EDITOR
            var settingsData = settings.atlases;
            
            var items = new List<AddressableAtlasData>(settingsData);
            var atlasAssets = AssetEditorTools.GetAssets<SpriteAtlas>();
            var collectedData = new Dictionary<string, AddressableAtlasData>();
            
            foreach (var atlas in atlasAssets)
            {
                var isAddressable = atlas.IsAddressable();
                if(!isAddressable) continue;
                
                var guid = atlas.GetGUID();

                var atlasData = new AddressableAtlasData()
                {
                    tag = atlas.tag,
                    guid = guid,
                    reference = new AssetReferenceT<SpriteAtlas>(guid),
                    enable = true,
                    preload = false,
                    isVariant = atlas.isVariant,
                    spriteCount = atlas.spriteCount,
                };

                collectedData[atlas.tag] = atlasData;
            }
            
            foreach (var atlasData in items)
            {
                var found = collectedData.TryGetValue(atlasData.guid, out var data);
                if (found) continue;
                settingsData.Remove(atlasData);
            }

            foreach (var atlasData in collectedData)
            {
                var found = false;
                
                foreach (var item in settingsData)
                {
                    if(!item.guid.Equals(atlasData.Key)) continue;
                    found = true;
                    var atlas = atlasData.Value;
                    item.tag = atlas.tag;
                    item.reference = atlas.reference;
                    item.spriteCount = atlas.spriteCount;
                    item.isVariant = atlas.isVariant;
                    
                    break;
                }
                
                if(!found) settingsData.Add(atlasData.Value);
            }

            this.MarkDirty();
#endif
        }
    }
}