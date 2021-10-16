#if ODIN_INSPECTOR

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UniModules.Editor;
using UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases;
using UnityEditor;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressableAtlasEditor
{
    public class AddressableAtlasWindow : OdinEditorWindow
    {
        #region static data

        [MenuItem("UniGame/Tools/Addressable Atlas Window")]
        public static void OpenWindow()
        {
            OpenWindow(AssetEditorTools.GetAsset<AddressableSpriteAtlasConfiguration>());
        }

        public static void OpenWindow(AddressableSpriteAtlasConfiguration configuration)
        {
            var window = GetWindow<AddressableAtlasWindow>();
            window.InitializeWindow(configuration);
            window.Show();
        }

        #endregion
        
        
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public AddressableSpriteAtlasConfiguration configuration;

        protected void InitializeWindow(AddressableSpriteAtlasConfiguration configurationAsset)
        {
            configuration = configurationAsset;
        }
    }
}


#endif
