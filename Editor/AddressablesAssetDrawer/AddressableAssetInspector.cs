using System.Collections.Generic;
using System.Reflection;
using UniModules.Editor;
using UniModules.UniCore.EditorTools.Editor.PropertiesDrawers;
using UniModules.UniGame.AddressableTools.Runtime.Attributes;
using UniModules.UniGame.Editor.DrawersTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesAssetDrawer
{
    using Object = UnityEngine.Object;

    [CustomPropertyDrawer(typeof(DrawAssetReferenceAttribute),true)]
    public class AddressableAssetInspector : PropertyDrawer
    {
        private const float FieldHeight = 20;
        private const string assetLabel = "Asset";
        private const string guiPropertyName = "m_AssetGUID";

        private static Dictionary<FieldInfo,PropertyDrawer> drawers = new Dictionary<FieldInfo, PropertyDrawer>();

        private bool isFoldoutOpen = false;

#if ODIN_INSPECTOR
        private bool odinSupport = true;  
#else 
        private bool odinSupport = false;
#endif
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => FieldHeight + base.GetPropertyHeight(property, label);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, new GUIContent(), property);

            if (property.isArray) {
                var targetRect = position;
                for (int i = 0; i < property.arraySize; i++) {
                    var item = property.GetArrayElementAtIndex(i);
                    DrawAssetReferenceInspector(targetRect,item,label);
                    targetRect.x += GetPropertyHeight(item, label);
                    position.height += targetRect.height;
                    EditorGUILayout.Separator();
                }
            }
            else {
                DrawAssetReferenceInspector(position, property, label);
            }


            EditorGUI.EndProperty();
        }

        public void DrawAssetReferenceInspector(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawOnGuiAssetReferenceInspector(position,property);

            DrawAssetReferenceDrawer(position, property, label);
        }

        private void DrawAssetReferenceDrawer(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo == null) return;

            position.y      += FieldHeight;
            position.height =  20f;

            if (!drawers.TryGetValue(fieldInfo,out var drawer)) {
                var drawerType = typeof(AssetReference);
                drawer     = fieldInfo.GetDrawer(drawerType);
                drawers[fieldInfo] = drawer;
            }
            
            drawer.OnGUI(position,property,label);
        }

        public void DrawOnGuiAssetReferenceInspector(Rect position, SerializedProperty property)
        {
            position.height = 20;

            var guidProperty = property.FindPropertyRelative(guiPropertyName);
            var assetGuid    = guidProperty.stringValue;
            if (string.IsNullOrEmpty(assetGuid))
            {
                EditorDrawerUtils.DrawDisabled(() => {
                    EditorGUI.ObjectField(position,assetLabel, null, typeof(Object),false); 
                }); 
                return;
            }
            
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            var mainType  = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            var asset     = AssetDatabase.LoadAssetAtPath(assetPath, mainType);

            if (!odinSupport)
                EditorDrawerUtils.DrawDisabled(() => EditorGUI.ObjectField(position,assetLabel, asset, asset != null 
                    ? asset.GetType() : null,false));    
            
            isFoldoutOpen = asset.DrawOdinPropertyWithFoldout(isFoldoutOpen);
        }

        public void DrawOnGuiAssetReferenceInspector(string assetGuid)
        {
            if (string.IsNullOrEmpty(assetGuid))
            {
                EditorDrawerUtils.DrawDisabled(() => {
                    EditorGUILayout.ObjectField(assetLabel, null, typeof(Object),false); 
                }); 
                return;
            }
            
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            var mainType  = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            var asset     = AssetDatabase.LoadAssetAtPath(assetPath, mainType);

            EditorDrawerUtils.DrawDisabled(() => {
                EditorGUILayout.ObjectField(assetLabel, asset, asset.GetType(),false);
            }); 
            
            isFoldoutOpen = asset.DrawOdinPropertyWithFoldout(isFoldoutOpen);
        }

    }
}
