#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using AI.HSM;

using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

[CustomPropertyDrawer(typeof(State), true)]
public class StateDrawer : PropertyDrawer {

    const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly Dictionary<Type, FieldInfo[]> _fieldCache = new Dictionary<Type, FieldInfo[]>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (property.managedReferenceValue == null) { return EditorGUIUtility.singleLineHeight; }

        SerializedProperty[] props = GetProps(property);

        float height = 0f;

        foreach (SerializedProperty prop in props) {
            if (prop != null) {
                height += EditorGUI.GetPropertyHeight(prop, true) + 2f;
            }
        }

        return height > 0 ? height : EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        if (property.managedReferenceValue != null) {
            SerializedProperty[] props = GetProps(property);

            foreach (SerializedProperty prop in props) {
                if (prop != null) {
                    float propHeight = EditorGUI.GetPropertyHeight(prop, true);

                    rect.height = propHeight;
                    EditorGUI.PropertyField(rect, prop, true);

                    rect.y += propHeight + 2f;
                }
            }
        } else {
            EditorGUI.LabelField(rect, "Null");
        }

        EditorGUI.EndProperty();
    }

    ///<summary>This is fairly nasty - try to cache reflection info where possible</summary>
    ///<param name="reference">Non null ref to object (will error out if null)</param>
    ///<returns>Array of properties in reverse order as for some reason they are backwards...</returns>
    private SerializedProperty[] GetProps(SerializedProperty reference) {
        object obj = reference.managedReferenceValue;
        if (obj == null) return Array.Empty<SerializedProperty>();

        var type = obj.GetType();

        if (!_fieldCache.TryGetValue(type, out var fields)) {
            fields = type.GetFields(flags).Reverse().ToArray();
            _fieldCache[type] = fields;
        }

        return fields
            .Select(f => reference.FindPropertyRelative(f.Name))
            .Where(p => p != null)
            .ToArray();
    }
}

#endif