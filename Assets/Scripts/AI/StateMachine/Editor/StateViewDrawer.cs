#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using AI.HSM;

[CustomPropertyDrawer(typeof(AIStateView))]
public class StateViewDrawer : PropertyDrawer {

    const float Padding = 5.0f;
    const float LabelJustification = 0.4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty key = property.FindPropertyRelative("Key");
        SerializedProperty parent = property.FindPropertyRelative("Parent");

        float width = position.width * 0.5f - Padding * 2;

        Rect keyRect = new Rect(position.x + Padding, position.y, width * LabelJustification, EditorGUIUtility.singleLineHeight);
        Rect parentRect = new Rect(position.x + position.width * 0.5f + Padding * 2, position.y, width * LabelJustification, EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(keyRect, "Key");
        keyRect.width = width - keyRect.width - Padding * 2;
        keyRect.x += width * LabelJustification + Padding * 2;
        EditorGUI.PropertyField(keyRect, key, GUIContent.none);

        EditorGUI.LabelField(parentRect, "Parent");
        parentRect.width = width - parentRect.width - Padding * 2;
        parentRect.x += width * LabelJustification + Padding * 2;
        EditorGUI.PropertyField(parentRect, parent, GUIContent.none);

        EditorGUI.EndProperty();
    }
}

#endif