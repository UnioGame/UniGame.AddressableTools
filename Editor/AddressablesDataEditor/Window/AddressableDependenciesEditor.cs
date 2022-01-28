#if ODIN_INSPECTOR

using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniModules.Editor;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
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
        public AddressablesDependenciesResolver dependencies;
        
        [Space]
        
        #endregion
        
        private Scene _scene;
        private ILifeTime _lifeTime;

        public void Initialize(ILifeTime lifeTime)
        {
            _lifeTime = lifeTime;
            
            configuration = AddressablesDependenciesConfiguration.Asset;
            addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            scenePath = AssetDatabase.GetAssetPath(configuration.sceneAsset);
            dependencies = new AddressablesDependenciesResolver(lifeTime,configuration.logPath.ToAbsoluteProjectPath());
        }

        [Button]
        public void CollectData()
        {
            if (!EditorApplication.isPlaying)
            {
                _scene = EditorSceneManager.OpenScene(scenePath, sceneMode);
                EditorApplication.EnterPlaymode();
            }

            CollectDataAsync().Forget();
        }

        private async UniTask CollectDataAsync()
        {
            await UniTask.WaitWhile(() => !EditorApplication.isPlaying).AttachExternalCancellation(_lifeTime.TokenSource);

            dependencies.ExecuteAsync().Forget();
        }
    }
}

#endif