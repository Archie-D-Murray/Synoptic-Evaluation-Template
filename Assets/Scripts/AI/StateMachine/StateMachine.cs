using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using Utilities;

namespace AI.HSM {
    public class StateMachine {
        private bool _started = false;
        public readonly Dictionary<State, State> InitialStates = new Dictionary<State, State>();
        public readonly Dictionary<State, HashSet<TransitionCondition>> StateTransitions = new Dictionary<State, HashSet<TransitionCondition>>();
        public readonly HashSet<TransitionCondition> AnyStateTransition = new HashSet<TransitionCondition>();
        public readonly HashSet<State> StatePath = new HashSet<State>(3);

        protected State _root;
        public State Root => _root;

        public StateMachine() {
            _root = null;
        }

        public StateMachine(State root) {
            _root = root;
        }

        public void SetRoot(State root) {
            _root = root;
        }

        public void Start() {
            if (_started) { return; }
            _started = true;
            Root.Enter();
            StatePath.Add(_root);
        }

        public void Tick(float deltaTime) {
            if (!_started) { Start(); }
            Root.Update(deltaTime);
            StatePath.Clear();
            for (State state = _root; state != null; state = state.ActiveChild) {
                StatePath.Add(state);
            }
        }

        public void FixedTick() {
            Root.FixedUpdate();
        }

        public void ChangeState(State from, State to) {
            if (from == to || from == null || to == null) { return; }
            State lca = State.LCA(from, to);
            for (State state = from; state != lca; state = state.Parent) { state.Exit(); }
            Stack<State> toStack = new Stack<State>();
            for (State state = to; state != lca; state = state.Parent) { toStack.Push(state); }
            while (toStack.Count > 0) { toStack.Pop().Enter(); }
        }

        public void AddAnyTransition(State to, IPredicate condition) {
            if (to == null) {
                Debug.LogWarning($"{Environment.StackTrace}\n[StateMachine]: To state was null");
                return;
            }
            AnyStateTransition.Add(new TransitionCondition(to, condition));
        }

        public void AddStateTransition(State from, State to, IPredicate condition) {
            if (from == null || to == null) {
                Debug.LogWarning($"{Environment.StackTrace}\n[StateMachine]: Got a null state in either to or from params: To: {Helpers.ClassNameOrNull(to)}, From: {Helpers.ClassNameOrNull(from)}");
                return;
            }
            AddState(from, new TransitionCondition(to, condition));
        }

        public void AddInitialState(State state, State initial) {
            if (InitialStates.ContainsKey(state)) {
                Debug.LogWarning($"Tried to set initial state of {state.GetType().Name} but already have entry - using previous initial value");
                return;
            }
            InitialStates.Add(state, initial);
        }

        private void AddState(State from, TransitionCondition condition) {
            if (StateTransitions.TryGetValue(from, out HashSet<TransitionCondition> transitions)) {
                transitions.Add(condition);
            } else {
                StateTransitions.Add(from, new HashSet<TransitionCondition>() { condition });
            }
        }
    }

    public class StateMachineBuilder {
        private readonly State _root;

        public StateMachineBuilder(State root) {
            _root = root;
        }

        public StateMachine Build() {
            StateMachine stateMachine = new StateMachine(_root);
            Wire(_root, stateMachine, new HashSet<State>());
            return stateMachine;
        }

        // This is gross but it works and anything else would be a massive pain - it essentially assigns the state machine to all states
        private void Wire(State state, StateMachine machine, HashSet<State> visited) {
            if (state == null || !visited.Add(state)) { return; }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            FieldInfo machineField = typeof(State).GetField(nameof(State.StateMachine), flags);
            if (machineField != null) { machineField.SetValue(state, machine); }

            foreach (FieldInfo field in state.GetType().GetFields(flags)) { // Get all fields of type AIState
                if (!typeof(State).IsAssignableFrom(field.FieldType)) { continue; }
                if (field.Name == nameof(State.Parent)) { continue; } // No infinite recursion
                State child = (State) field.GetValue(state);
                if (child == null) { continue; }
                if (!ReferenceEquals(child.Parent, state)) { continue; }
                Wire(child, machine, visited); // Recurse over children
            }
        }
    }
}