#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using AI.HSM;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;

[CustomEditor(typeof(StateMachineContext))]
public class StateMachineContextEditor : Editor {
    private VisualElement _root;
    private Label _statePath;
    private VisualElement _stateStack;

    public override VisualElement CreateInspectorGUI() {
        _root = new VisualElement();

        _statePath = new Label("State Path:");
        _statePath.style.unityFontStyleAndWeight = FontStyle.Bold;
        _root.Add(_statePath);

        _stateStack = new VisualElement();
        _stateStack.style.marginTop = 6;
        _root.Add(_stateStack);

        SerializedProperty iterator = serializedObject.GetIterator();
        if (iterator.NextVisible(true)) {
            do {
                if (iterator.name == "DebugStates") continue;

                PropertyField field = new PropertyField(iterator.Copy());
                field.Bind(serializedObject);
                _root.Add(field);

            } while (iterator.NextVisible(false));
        }

        _root.schedule.Execute(UpdateView).Every(100);

        return _root;
    }


    private void UpdateView() {
        StateMachineContext ctx = target as StateMachineContext;

        if (ctx == null) {
            return;
        }

        if (!Application.isPlaying || ctx.StateMachine == null) {
            return;
        }

        List<State> path = ctx.StateMachine.Root
            .GetLeaf()
            .PathToRoot()
            .Reverse()
            .ToList();

        _statePath.text = "State Path: " +
            string.Join(" > ", path.Select(s => s.GetType().Name));

        RebuildStateStack(path);
    }

    private void RebuildStateStack(List<State> path) {
        _stateStack.Clear();

        foreach (State state in path) {
            Foldout foldout = new Foldout();
            foldout.text = state.GetType().Name;
            foldout.value = true;

            IEnumerable<FieldInfo> fields = GetAllFields(state.GetType());

            foreach (FieldInfo field in fields) {
                object value = field.GetValue(state);

                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;

                Label nameLabel = new Label(field.Name + ": ");
                nameLabel.style.width = 140;

                Label valueLabel = new Label(Format(value));

                row.Add(nameLabel);
                row.Add(valueLabel);

                foldout.Add(row);
            }

            _stateStack.Add(foldout);
        }
    }

    private static IEnumerable<FieldInfo> GetAllFields(System.Type type) {
        const System.Reflection.BindingFlags flags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.DeclaredOnly;

        while (type != null && type != typeof(object)) {
            foreach (FieldInfo f in type.GetFields(flags))
                yield return f;

            type = type.BaseType;
        }
    }

    private string Format(object value) {
        if (value == null) return "null";
        if (value is State s) return s.GetType().Name;
        return value.ToString();
    }
}

#endif