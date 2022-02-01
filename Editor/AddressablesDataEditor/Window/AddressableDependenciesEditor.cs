#if ODIN_INSPECTOR

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using System;
    using System.Collections;
    using Sirenix.OdinInspector;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor.Window;
    using Unity.EditorCoroutines.Editor;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.SceneManagement;
    
    [Serializable]
    public class AddressableDependenciesEditor
    {
        #region inspector
        
        [FoldoutGroup(nameof(configuration))]
        [InlineEditor()]
        public AddressablesDependenciesConfiguration configuration;
        
        [FoldoutGroup(nameof(configuration))]
        public AddressableAssetSettings addressableSettings;
        
        [FoldoutGroup(nameof(configuration))]
        public string scenePath;
        
        [FoldoutGroup(nameof(configuration))]
        public OpenSceneMode sceneMode = OpenSceneMode.Single;
        
        [InlineProperty]
        [HideLabel]
        public AddressablesDependenciesView dependenciesView = new AddressablesDependenciesView();

        #endregion

        public void Initialize()
        {
            configuration = AddressablesDependenciesConfiguration.Asset;
            configuration.OnValidate();
            
            addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            scenePath = AssetDatabase.GetAssetPath(configuration.sceneAsset);
            dependenciesView.Initialize(configuration);
        }

        [Button]
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        public void CollectData()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene(scenePath, sceneMode);
                EditorApplication.EnterPlaymode();
            }

            EditorCoroutineUtility.StartCoroutine(CollectDataAsync(), this);
        }

        [Button]
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        public void Clear() => dependenciesView.Reset();

        private IEnumerator CollectDataAsync()
        {
            while (!EditorApplication.isPlaying)
                yield return null;
            
            dependenciesView.CollectAddressableData();
        }
    }
}

#endif