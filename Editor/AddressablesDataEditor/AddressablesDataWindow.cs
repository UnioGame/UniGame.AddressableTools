#if ODIN_INSPECTOR

using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UniModules.UniCore.Runtime.DataFlow;
using UnityEditor;

namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDataEditor
{
    using UnityEngine;
    
    public class AddressablesDataWindow : OdinEditorWindow
    {
        
    }

    [Serializable]
    public class AddressablesDataView : IDisposable
    {
        #region inspector
        
        public const string assetDataGroup = "input:";

        [VerticalGroup(assetDataGroup)]
        public Object target;

        [VerticalGroup(assetDataGroup)]
        public string guid;

        [VerticalGroup(assetDataGroup)]
        [OnValueChanged(nameof(OnSelectionStatusChanged))]
        public bool enableSelection = true;

        #endregion

        private LifeTimeDefinition _lifeTime = new LifeTimeDefinition();
        
        public void UpdateSelection()
        {
            
        }
        
        public void OnSelectionStatusChanged()
        {
            Selection.selectionChanged -= UpdateSelection;
            if(enableSelection)
                Selection.selectionChanged += UpdateSelection;
        }

        public void Dispose() => _lifeTime.Terminate();
    }

    [Serializable]
    public class AddressableAssetData
    {
        
    }
    
}

#endif