using System.IO;
using UniCore.Runtime.ProfilerTools;
using UniRx;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableSpriteAtlasManager
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AddressableExtensions.Editor;
    using UniModules.Editor;
    using Runtime.AssetReferencies;
    using Runtime.SpriteAtlases;
    using UniCore.Runtime.ProfilerTools;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine.U2D;
    
    public static class AddressableSpriteAtlasesEditorHandler 
    {
        private const int FastModeIndex = 0;

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            MessageBroker.Default
                .Receive<UpdateAddressableAtlasesMessage>()
                .Subscribe(x => Reimport());
        }
        
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
            if (!AddressableAssetSettingsDefaultObject.SettingsExists) {
                GameLog.LogError("Addressable Asset Settings doesn't exist!");
                return;
            }

            EditorCoroutineUtility.StartCoroutineOwnerless(UpdateMode());

            AddressableAssetSettings.OnModificationGlobal += OnModification;
        }


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
            var atlasManagers = AssetEditorTools.GetAssets<AddressableSpriteAtlasConfiguration>();
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
            var atlasManagers = AssetEditorTools.GetAssets<AddressableSpriteAtlasConfiguration>();
            
            foreach (var manager in atlasManagers) {
                manager.isFastMode = isFastMode;
                manager.MarkDirty();
                
                GameLog.Log($"Set fast mode [{isFastMode}] to {manager.name}");
            }
        }

        private static void SetupMap(AddressableSpriteAtlasConfiguration handler, IReadOnlyList<AssetReferenceSpriteAtlas> atlases)
        {
            var map = handler.atlasesTagsMap;
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
