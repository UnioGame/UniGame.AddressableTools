#if ODIN_INSPECTOR

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UniModules.Editor;
using UniGame.AddressableTools.Runtime.SpriteAtlases;
using UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract;
using UnityEditor;
using UnityEngine;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressableAtlasEditor
{
    public class AddressableAtlasWindow : OdinEditorWindow
    {
        #region static data

        [MenuItem("UniGame/Tools/Addressable Atlas Window")]
        public static void OpenWindow()
        {
            var window = GetWindow<AddressableAtlasWindow>();
            window.Reload();
            window.Show();;
        }

        #endregion
        
        [InlineProperty]
        [SerializeReference]
        [HideLabel]
        public IAddressableAtlasService atlasService;

        [Button("Reload Config")]
        public void Reload()
        {
            atlasService = AddressableSpriteAtlasAsset.AtlasService;
        }
        
        protected void InitializeWindow(AddressableSpriteAtlasAsset configurationAsset) => Reload();
    }
}


#endif
