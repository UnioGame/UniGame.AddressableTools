#if ODIN_INSPECTOR

using System;
using System.Collections;
using Sirenix.OdinInspector;
using UniModules.Editor;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
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
        
        [TitleGroup("addressables data")]
        [InlineProperty]
        [HideLabel]
        public AddressablesDependenciesView dependencies;
        
        [Space]
        
        #endregion
        
        private ILifeTime _lifeTime;

        public void Initialize(ILifeTime lifeTime)
        {
            _lifeTime = lifeTime;
            
            configuration = AddressablesDependenciesConfiguration.Asset;
            configuration.OnValidate();
            
            addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            scenePath = AssetDatabase.GetAssetPath(configuration.sceneAsset);
            dependencies = new AddressablesDependenciesView(lifeTime,configuration.logPath.ToAbsoluteProjectPath(),configuration.filters);
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
        public void Clear() => dependencies.Reset();

        private IEnumerator CollectDataAsync()
        {
            while (!EditorApplication.isPlaying)
                yield return null;
            
            dependencies.CollectAddressableData();
        }
    }
}

#endif