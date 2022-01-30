using System.Collections.Generic;

#if ODIN_INSPECTOR

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using Sirenix.OdinInspector;
    using System.IO;
    using UniModules.Editor;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UniModules.UniGame.Core.Editor.EditorProcessors;
    using UnityEngine;

    [CreateAssetMenu(menuName = "UniGame/Addressables/" + nameof(AddressablesDependenciesConfiguration))]
    [GeneratedAssetInfo("Editors/Editor")]
    public class AddressablesDependenciesConfiguration : GeneratedAsset<AddressablesDependenciesConfiguration>
    {
        public Object sceneAsset;

        public string logPath = "Logs/addressables_dependencies.log";

        [FoldoutGroup("filters")]
        [SerializeReference]
        [InlineProperty]
        [ListDrawerSettings()]
        public List<IAddressableDataFilter> filters = new List<IAddressableDataFilter>()
        {
            new LocalToRemoteDependenciesFilter(),
            new EntryDependenciesFilter(),
        };
        
        [Button("Validate")]
        public void OnValidate()
        {
            filters.RemoveAll(x => x == null);
            
            if (sceneAsset) return;
            var path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path))
                return;
            
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            scene.name = "empty_scene_asset";
            var folder = Path.GetDirectoryName(path);
            var assetPath = folder.CombinePath(scene.name);
            var targetPath = assetPath + ".unity";
            
            EditorSceneManager.SaveScene(scene, targetPath);

            sceneAsset = AssetDatabase.LoadAssetAtPath<Object>(targetPath);

            this.MarkDirty();
        }
    }
}

#endif