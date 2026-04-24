
#if UNITY_EDITOR
using System;

using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


using UnityEditor;

// [CustomPropertyDrawer(typeof(AttackAdaptor))]
// public class AttackAdapterDrawer : PropertyDrawer {
//
//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
//         return EditorGUIUtility.singleLineHeight + 2;
//     }
//
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//         EditorGUI.BeginProperty(position, label, property);
//         SerializedProperty normalizedTime = property.FindPropertyRelative("_normalizedTime");
//
//         Rect nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
//         Rect normalizedRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
//         if (normalizedTime == null) {
//             EditorGUI.LabelField(normalizedRect, "Null");
//         } else {
//             EditorGUI.PropertyField(normalizedRect, normalizedTime);
//         }
//
//         EditorGUI.EndProperty();
//     }
// }

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorPropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        var visualElement = new VisualElement();

        var propertyField = new PropertyField();
        propertyField.BindProperty(property);
        propertyField.label = " ";

        if (property.propertyType != SerializedPropertyType.ManagedReference) {
            visualElement.Add(propertyField);
            return visualElement;
        }

        var types = GetTypes(fieldInfo, property);

        var dropdownField = new TypePopupField(property, types);
        visualElement.Add(dropdownField);

        visualElement.Add(propertyField);

        return visualElement;
    }

    private static bool IsCollection(Type fieldType) {
        if (fieldType.IsArray) {
            return true;
        }

        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)) {
            return true;
        }

        return false;
    }

    private static List<Type> GetTypes(FieldInfo fieldInfo, SerializedProperty property) {
        var value = property.managedReferenceValue;
        Type currentType = value?.GetType();

        Type fieldType = fieldInfo.FieldType;
        Type baseType;

        bool isCollection = IsCollection(fieldType);

        var types = new List<Type>() { currentType, null };

        if (!fieldType.IsAbstract && !isCollection) {
            types.Add(fieldType);
        }

        if (isCollection) {
            baseType = fieldType.GetGenericArguments()[0];
            if (!baseType.IsAbstract) {
                types.Add(baseType);
            }
        } else {
            baseType = fieldType;
        }

        var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType);
        foreach (var derivedType in derivedTypes) {
            types.Add(derivedType);
        }

        return types;
    }


    public class TypePopupField : PopupField<Type> {

        private readonly SerializedProperty _property;

        public TypePopupField(SerializedProperty property, List<Type> types) : base(property.displayName, types, 0, GetTypeName, GetTypeName) {
            _property = property;
            this.RegisterValueChangedCallback(OnValueSelected);
        }

        private void OnValueSelected(ChangeEvent<Type> changeEvent) {
            Type selectedType = changeEvent.newValue;
            if (selectedType == null) {
                _property.managedReferenceValue = null;
                _property.serializedObject.ApplyModifiedProperties();
            } else {
                var constructor = selectedType.GetConstructor(Type.EmptyTypes);
                if (constructor != null) {
                    var value = constructor.Invoke(null);
                    _property.managedReferenceValue = value;
                    _property.serializedObject.ApplyModifiedProperties();
                } else {
                    Debug.LogWarning($"Selected Type {selectedType.Name} does not have a parameterless constructor. Cannot assign instance of type.");
                }
            }
        }

        private static string GetTypeName(Type type) {
            if (type == null) {
                return "Null";
            } else {
                return ObjectNames.NicifyVariableName(type.Name);
            }
        }

    }

}

#endif