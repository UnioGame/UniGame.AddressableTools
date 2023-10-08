namespace UniGame.AddressableTools.Runtime.AssetReferencies
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

    [Serializable]
    public class AddressableValue<TValue>
        where TValue : Object
    {
        [HideLabel]
        [OnValueChanged(nameof(UpdateView))]
        public AssetReferenceT<TValue> reference;
        
        [NonSerialized]
        [HideLabel]
        [InlineEditor]
        [OnInspectorGUI]
        private Object _value;

        public Object EditorValue
        {
            get
            {
#if UNITY_EDITOR
                return reference.editorAsset;
#endif
                return null;
            }
        }
        
        [Button]
        [OnInspectorInit]
        public void UpdateView()
        {
#if UNITY_EDITOR
            _value = reference.editorAsset;
#endif
        }
    }
    
    
    [Serializable]
    public class AddressableValue
    {
        [HideLabel]
        [OnValueChanged(nameof(UpdateView))]
        public AssetReference reference;
        
        [NonSerialized]
        [HideLabel]
        [InlineEditor]
        [OnInspectorGUI]
        private Object _value;

        [Button]
        public void UpdateView()
        {
#if UNITY_EDITOR
            _value = reference.editorAsset;
#endif
        }
    }
}