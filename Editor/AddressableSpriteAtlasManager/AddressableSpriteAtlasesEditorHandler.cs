using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniCore.Runtime.ProfilerTools;
using UniModules.Editor;
using UniModules.UniGame.AddressableExtensions.Editor;
using UniModules.UniGame.AddressableTools.Runtime.AssetReferencies;
using UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases;
using UniRx;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.U2D;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressableSpriteAtlasManager
{
    public static class AddressableSpriteAtlasesEditorHandler 
    {
        private const int FastModeIndex = 0;

        
        [MenuItem("UniGame/Addressables/Reimport Atlases")]
        public static void Reimport()
        {
            var atlases = AssetEditorTools.GetAssets<SpriteAtlas>();
            var addressableAtlases = atlases.
                Where(x => x.IsInAnyAddressableAssetGroup()).
                Select(x => new AssetReferenceSpriteAtlas(AssetEditorTools.GetGUID(x))).
                ToList();

            UpdateAtlasMap(addressableAtlases);
            UpdateAtlasStates();
        }

        [InitializeOnLoadMethod]
        private static void SubscribeOnAddressableMode()
        {
            MessageBroker.Default
                .Receive<UpdateAddressableAtlasesMessage>()
                .RxSubscribe(x => Reimport());
            
            if (!AddressableAssetSettingsDefaultObject.SettingsExists) {
                GameLog.LogError("Addressable Asset Settings doesn't exist!");
                return;
            }

            EditorCoroutineUtility.StartCoroutineOwnerless(UpdateMode());
            AddressableAssetSettings.OnModificationGlobal += OnModification;
        }

        [MenuItem("UniGame/Addressables/Reimport All Atlases States")]
        private static void UpdateAtlasStates()
        {
            var atlasHandles = AssetEditorTools.GetAssets<AddressableAtlasesStateAsset>();
            foreach (var atlasesStateAsset in atlasHandles)
            {
                var atlases = atlasesStateAsset.atlases;
                var atlasesLocations = atlasesStateAsset.assetFolders.Where(Directory.Exists).ToArray();
                var atlasTags = AssetEditorTools
                    .GetAssets<SpriteAtlas>(atlasesLocations)
                    .Select(x => x.tag)
                    .ToList();

                atlasTags.AddRange(atlasesStateAsset.assets
                    .Where(File.Exists)
                    .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
                    .Where(x => x)
                    .Select(x => x.tag));
                
                atlases.atlasTags.Clear();
                atlases.atlasTags.AddRange(atlasTags);
                atlasesStateAsset.MarkDirty();
            }
        }
        
        private static void UpdateAtlasMap(List<AssetReferenceSpriteAtlas> atlases)
        {
            var atlasManagers = AssetEditorTools.GetAssets<AddressableSpriteAtlasAsset>();
            foreach (var manager in atlasManagers) {
                SetupMap(manager,atlases);
                manager.MarkDirty();
            }
        }
        
        private static IEnumerator UpdateMode()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            while (settings == null) {
                settings = AddressableAssetSettingsDefaultObject.Settings;
                yield return null;
            }
            
            var isFastMode = settings.ActivePlayModeDataBuilderIndex == FastModeIndex;
            SetFastModeToManagers(isFastMode);
        }

        private static void OnModification(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent modificationEvent, object o)
        {
            if (modificationEvent == AddressableAssetSettings.ModificationEvent.ActivePlayModeScriptChanged) {
                var isFastMode = settings.ActivePlayModeDataBuilderIndex == FastModeIndex;
                SetFastModeToManagers(isFastMode);
            }
        }

        private static void SetFastModeToManagers(bool isFastMode)
        {
            var atlasManagers = AssetEditorTools.GetAssets<AddressableSpriteAtlasAsset>();
            
            foreach (var manager in atlasManagers) {
                manager.settings.isFastMode = isFastMode;
                manager.MarkDirty();
                GameLog.Log($"Set fast mode [{isFastMode}] to {manager.name}");
            }
        }

        private static void SetupMap(AddressableSpriteAtlasAsset handler, IReadOnlyList<AssetReferenceSpriteAtlas> atlases)
        {
            var map = handler.settings.atlasesTagsMap;
            map.Clear();

            foreach (var atlasRef in atlases) {
                var atlas = atlasRef.editorAsset;
                var tag = atlas.tag;
                
                if (map.ContainsKey(atlas.tag))
                {
                    var atlasReference = map[atlas.tag];
                    var assetReference = atlasReference.assetReference;
                    if (assetReference.editorAsset == atlas && string.Equals(tag, atlasReference.tag))
                    {
                        continue;
                    }
                }

                map[atlas.tag] = new AtlasReference()
                {
                    tag = atlas.tag,
                    assetReference = atlasRef
                };
                
            }
            
            handler.Validate();
        }
    }
}
