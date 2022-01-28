using System;
using Sirenix.OdinInspector;
using UniModules.UniCore.Runtime.DataFlow;
using UnityEditor;
using UnityEngine;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor
{
    using Object = UnityEngine.Object;
    
    [Serializable]
    public class AddressableDataView : IDisposable
    {
        public const string Settings = "settings";

        #region inspector

        [FoldoutGroup(Settings)]
        [OnValueChanged(nameof(OnSelectionStatusChanged))]
        public bool enableSelection = true;

        [InlineProperty]
        [HideLabel]
        [BoxGroup(nameof(asset))]
        public AddressableAssetView assetView = new AddressableAssetView();
                
        [Space]
        [Space]
        [HideLabel]
        [InlineProperty]
        [TitleGroup(nameof(dependences))]
        public AddressableDependencyView dependences = new AddressableDependencyView();

        
        #endregion

        private Object asset;

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
            Initialize(selectedAsset);
        }

        [Button]
        public void Refresh()
        {
            Initialize(asset);
        }
        
        public AddressableDataView Initialize(Object target)
        {
            assetView.Initialize(target);
            dependences.Initialize(target);

            return this;
        }

        public void Reset()
        {
            assetView.Reset();
            dependences.Reset();
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