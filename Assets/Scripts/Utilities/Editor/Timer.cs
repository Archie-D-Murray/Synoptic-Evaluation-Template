#if UNITY_EDITOR

using UnityEditor;

using UnityEngine;

using Utilities;

[CustomPropertyDrawer(typeof(CountDownTimer), true)]
public class TimerDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // 2 fields + progress bar + spacing
        return (EditorGUIUtility.singleLineHeight * 3) + 8;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        // Draw foldout label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        SerializedProperty initialTime = property.FindPropertyRelative("_initialTime");
        SerializedProperty remainingTime = property.FindPropertyRelative("_time");
        SerializedProperty active = property.FindPropertyRelative("_isRunning");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        Rect initialRect = new Rect(position.x, position.y, position.width, lineHeight);
        Rect remainingRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
        Rect progressRect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width, lineHeight);

        // Draw fields
        EditorGUI.PropertyField(initialRect, initialTime);
        EditorGUI.PropertyField(remainingRect, remainingTime);

        // Calculate progress
        float init = initialTime.floatValue;
        float remain = remainingTime.floatValue;

        float progress = 1f;
        if (init > 0f) { progress = Mathf.Clamp01(1f - (remain / init)); }


        // Draw progress bar
        bool old = GUI.enabled;
        GUI.enabled = active.boolValue;
        EditorGUI.ProgressBar(progressRect, progress, $"Progress: {progress:P0}");
        GUI.enabled = old;

        EditorGUI.EndProperty();
    }
}

#endif