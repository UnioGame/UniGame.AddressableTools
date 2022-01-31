#if ODIN_INSPECTOR

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
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
        
        [InlineProperty]
        [HideLabel]
        public AddressableDependenciesEditor editor = new AddressableDependenciesEditor();

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

#endif
}