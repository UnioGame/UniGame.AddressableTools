#if ODIN_INSPECTOR
#if UNITY_EDITOR


namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor.Window;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UniModules.UniCore.Runtime.DataFlow;
    using UnityEditor;
    
    public class AddressablesDependenciesWindow : OdinEditorWindow
    {
        #region static data

        [MenuItem("UniGame/Tools/Addressables/Addressables Dependencies Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<AddressablesDependenciesWindow>();
            window.Show();
        }
    
        #endregion
    
        #region inspector
        
        [TabGroup("Addressables")]
        [InlineProperty]
        [HideLabel]
        public AddressableDependenciesEditor editor = new AddressableDependenciesEditor();

        
        [TabGroup("Single Asset")]
        [InlineProperty]
        [HideLabel]
        public SingleAssetDependenciesView singleAssetView = new SingleAssetDependenciesView();

        
        #endregion
        
        private LifeTimeDefinition _lifeTime = new LifeTimeDefinition();
        
        public bool IsPlaying => EditorApplication.isPlaying;
        
        [Button]
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup()]
        [DisableIf(nameof(IsPlaying))]
        public void Reload()
        {
            _lifeTime.Release();
            editor = new AddressableDependenciesEditor();
            editor.Initialize();
        } 
        
        protected override void Initialize()
        {
            base.Initialize();
            Reload();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _lifeTime.Terminate();
        }
    }

}

#endif
#endif