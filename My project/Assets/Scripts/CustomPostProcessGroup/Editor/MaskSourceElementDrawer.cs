using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaskGenerator
{
    /// <summary>
    /// PropertyDrawer for each list element of List&lt;MaskSourceBase&gt;.
    /// Shows runtime type name as label and allows changing type via dropdown.
    /// </summary>
    [CustomPropertyDrawer(typeof(MaskSourceBase), true)]
    public class MaskSourceElementDrawer : PropertyDrawer
    {
        private static readonly Type k_DefaultType = typeof(StencilMaskSource);
        private static List<Type> s_DerivedTypes;
        private static GUIContent[] s_TypeLabels;

        private static void EnsureTypes()
        {
            if (s_DerivedTypes != null)
                return;
            var baseType = typeof(MaskSourceBase);
            s_DerivedTypes = baseType.Assembly.GetTypes()
                .Where(t => t != baseType && baseType.IsAssignableFrom(t) && !t.IsAbstract)
                .OrderBy(t => t == k_DefaultType ? 0 : 1)
                .ThenBy(t => t.Name)
                .ToList();
            s_TypeLabels = s_DerivedTypes.Select(t => new GUIContent(t.Name)).ToArray();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnsureTypes();
            if (s_DerivedTypes.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var obj = property.managedReferenceValue;
            if (obj == null)
                ApplyDefaultType(property);
            obj = property.managedReferenceValue;

            var currentType = obj?.GetType();
            var typeIndex = currentType != null ? s_DerivedTypes.IndexOf(currentType) : 0;
            if (typeIndex < 0)
                typeIndex = 0;

            label.text = currentType != null ? currentType.Name : "(Not set)";

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            var headerRect = new Rect(position.x, position.y, position.width, lineHeight);
            var dropdownRect = new Rect(headerRect.xMax - 120f, headerRect.y, 120f, lineHeight);
            var labelRect = new Rect(headerRect.x, headerRect.y, Math.Max(0, headerRect.width - 120f - 4f), lineHeight);

            EditorGUI.LabelField(labelRect, label);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(dropdownRect, typeIndex, s_TypeLabels);
            if (EditorGUI.EndChangeCheck() && newIndex != typeIndex && newIndex >= 0 && newIndex < s_DerivedTypes.Count)
            {
                var newType = s_DerivedTypes[newIndex];
                Undo.RecordObject(property.serializedObject.targetObject, "Change MaskSource Type");
                try
                {
                    property.managedReferenceValue = Activator.CreateInstance(newType);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                property.serializedObject.ApplyModifiedProperties();
                return;
            }

            var childY = headerRect.y + lineHeight + spacing;
            var childRect = new Rect(position.x, childY, position.width, position.height - (lineHeight + spacing));
            DrawChildren(childRect, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            EnsureTypes();
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float header = lineHeight + spacing;
            float childrenHeight = GetChildrenHeight(property);
            return header + childrenHeight;
        }

        private static void ApplyDefaultType(SerializedProperty property)
        {
            if (s_DerivedTypes.Count == 0)
                return;
            var targetType = s_DerivedTypes[0];
            Undo.RecordObject(property.serializedObject.targetObject, "Initialize MaskSource");
            try
            {
                property.managedReferenceValue = Activator.CreateInstance(targetType);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void DrawChildren(Rect position, SerializedProperty property)
        {
            var end = property.GetEndProperty();
            var it = property.Copy();
            if (!it.Next(true))
                return;
            var startY = position.y;
            var x = position.x;
            var w = position.width;
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = startY;
            do
            {
                if (SerializedProperty.EqualContents(it, end))
                    break;
                var h = EditorGUI.GetPropertyHeight(it, true);
                var rect = new Rect(x, currentY, w, h);
                EditorGUI.PropertyField(rect, it, true);
                currentY += h + spacing;
            } while (it.Next(false));
        }

        private static float GetChildrenHeight(SerializedProperty property)
        {
            var end = property.GetEndProperty();
            var it = property.Copy();
            if (!it.Next(true))
                return 0f;
            float h = 0f;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            do
            {
                if (SerializedProperty.EqualContents(it, end))
                    break;
                h += EditorGUI.GetPropertyHeight(it, true) + spacing;
            } while (it.Next(false));
            return h;
        }
    }
}
