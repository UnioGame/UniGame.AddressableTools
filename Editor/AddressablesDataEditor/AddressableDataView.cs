using System;
using Sirenix.OdinInspector;
using UniModules.Editor;
using UniModules.UniCore.Runtime.DataFlow;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableDataEditor
{
    [Serializable]
    public class AddressableDataView : IDisposable
    {
        #region inspector
        
        public const string assetDataGroup = "input:";

        [InlineEditor]
        [VerticalGroup(assetDataGroup)]
        [OnValueChanged(nameof(Refresh))]
        public Object asset;

        [VerticalGroup(assetDataGroup)]
        public string guid;

        [VerticalGroup(assetDataGroup)]
        [OnValueChanged(nameof(OnSelectionStatusChanged))]
        public bool enableSelection = true;

        [Space]
        [HideLabel]
        [InlineProperty]
        public AddressableDependencyView dependences = new AddressableDependencyView();
        
        #endregion

        private LifeTimeDefinition _lifeTime = new LifeTimeDefinition();

        public AddressableDataView Initialize()
        {
            OnSelectionStatusChanged();
            Refresh();
            return this;
        }
        
        public void UpdateSelection()
        {
            var selectedAsset = Selection.activeObject;
            if (selectedAsset == null) return;
            if (selectedAsset == asset) return;
            Initialize(asset);
        }

        public void Refresh()
        {
            Initialize(asset);
        }
        
        public AddressableDataView Initialize(Object target)
        {
            asset = target;
            guid = asset == null ? string.Empty : asset.GetGUID();
            dependences.UpdateView(asset);
            return this;
        }
        
        public void OnSelectionStatusChanged()
        {
            Selection.selectionChanged -= UpdateSelection;
            if(enableSelection)
                Selection.selectionChanged += UpdateSelection;
        }

        public void Dispose() => _lifeTime.Terminate();
    }
}