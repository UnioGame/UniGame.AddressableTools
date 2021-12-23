#if ODIN_INSPECTOR

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableDataEditor
{
    public class AddressablesDataWindow : OdinEditorWindow
    {
        #region static data

        [MenuItem("UniGame/Tools/Addressable Data Window")]
        public static void OpenWindow()
        {
            var window = GetWindow<AddressablesDataWindow>();
            window.Show();;
        }

        #endregion

        #region inspector

        [InlineProperty]
        [HideLabel]
        [TitleGroup("addressables data")]
        public AddressableDataView view = new AddressableDataView();

        #endregion

        protected override void Initialize()
        {
            base.Initialize();
            view = new AddressableDataView().Initialize();
        }
    }

    [Serializable]
    public class AddressableResourceData
    {
        public string referenceId;
        public IResourceLocation location;
        public bool isRemote;

        public List<AddressableResourceData> dependences = new List<AddressableResourceData>();
    }
}

#endif