#if ODIN_INSPECTOR

using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniModules.Editor;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    [Serializable]
    public class AddressableDependenciesEditor
    {
        #region inspector
        
        [TitleGroup(nameof(configuration))]
        [InlineEditor()]
        public AddressablesDependenciesConfiguration configuration;
        
        public AddressableAssetSettings addressableSettings;
        public string scenePath;
        public OpenSceneMode sceneMode = OpenSceneMode.Single;
        
        [TitleGroup(nameof(dependencies))]
        [InlineProperty]
        [HideLabel]
        public AddressablesDependenciesView dependencies;
        
        [Space]
        
        #endregion
        
        private Scene _scene;
        private ILifeTime _lifeTime;

        public void Initialize(ILifeTime lifeTime)
        {
            _lifeTime = lifeTime;
            
            configuration = AddressablesDependenciesConfiguration.Asset;
            configuration.OnValidate();
            
            addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            scenePath = AssetDatabase.GetAssetPath(configuration.sceneAsset);
            dependencies = new AddressablesDependenciesView(lifeTime,configuration.logPath.ToAbsoluteProjectPath());
        }

        [Button]
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        public void CollectData()
        {
            if (!EditorApplication.isPlaying)
            {
                _scene = EditorSceneManager.OpenScene(scenePath, sceneMode);
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
            {
                yield return null;
            }

            dependencies.CollectAddressableData();
        }
    }
}

#endif