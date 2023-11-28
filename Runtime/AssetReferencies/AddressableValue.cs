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
        public AssetReferenceT<TValue> reference = new(string.Empty);
        
        [NonSerialized]
        [HideLabel]
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [FoldoutGroup("reference")]
        [ShowIf(nameof(HasValue))]
        [OnInspectorGUI]
        private Object _value;

        public TValue EditorValue
        {
            get
            {
#if UNITY_EDITOR
                return reference.editorAsset;
#endif
                return null;
            }
        }
        
        public bool HasValue => reference != null && reference.RuntimeKeyIsValid();
        
        [Button]
        [OnInspectorInit]
        public void UpdateView()
        {
#if UNITY_EDITOR
            _value = reference.editorAsset;
#endif
        }
        
        public static explicit operator AssetReferenceT<TValue>(AddressableValue<TValue> v)
        {
            return v.reference;
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
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [FoldoutGroup("reference")]
        [ShowIf(nameof(HasValue))]
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
        
        public bool HasValue => reference != null && reference.RuntimeKeyIsValid();
        
        [Button]
        [OnInspectorInit]
        public void UpdateView()
        {
#if UNITY_EDITOR
            _value = reference.editorAsset;
#endif
        }
        
        public static explicit operator AssetReference(AddressableValue v)
        {
            return v.reference;
        }
    }
}