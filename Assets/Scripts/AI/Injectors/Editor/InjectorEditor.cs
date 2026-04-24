#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using AI.Injectors;

[CustomPropertyDrawer(typeof(IStateInjector), true)]
public class InjectorEditor : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, EditorGUIUtility.GetControlID(FocusType.Passive), label);

        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        if (property.managedReferenceValue == null) {
            EditorGUI.LabelField(rect, "Null");
        } else {
            EditorGUI.LabelField(rect, property.managedReferenceValue.GetType().Name);
        }
    }
}

#endif