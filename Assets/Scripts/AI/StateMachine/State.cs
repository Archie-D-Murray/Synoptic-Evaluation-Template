using System;
using System.Collections.Generic;
using System.Linq;

namespace AI.HSM {
    [Serializable]
    public abstract class State {
        public readonly StateMachine StateMachine;
        public readonly State Parent;
        public State ActiveChild;

        protected State(StateMachine stateMachine, State parent) {
            StateMachine = stateMachine;
            Parent = parent;
        }

        ///<summary>The child state to add to path (null if leaf) when entering for first time</summary>
        ///<returns>Null if leaf state or first child to transition to</returns>
        protected virtual State GetInitialState() {
            return StateMachine.InitialStates.GetValueOrDefault(this);
        }

        ///<summary>Uses transition tables to determine next state - will handle unrelated state parents</summary>
        ///<returns>Null if staying in current state or new state to move to</returns>
        protected virtual State GetTransition() {
            State transition = null;
            if (StateMachine.StateTransitions.ContainsKey(this)) {
                foreach (TransitionCondition transitionCondition in StateMachine.StateTransitions.GetValueOrDefault(this)) {
                    if (StateMachine.StatePath.Contains(transitionCondition.To)) { continue; }
                    if (transitionCondition.Evaluate()) {
                        transition = transitionCondition.To;
                    }
                }
            }
            foreach (TransitionCondition transitionCondition in StateMachine.AnyStateTransition) {
                if (StateMachine.StatePath.Contains(transitionCondition.To)) { continue; }
                if (transitionCondition.Evaluate()) {
                    transition = transitionCondition.To;
                }
            }
            return transition;
        }

        ///<summary>Called when state is entered</summary>
        protected virtual void OnEnter() { }
        ///<summary>Called with FixedUpdate through state machine</summary>
        protected virtual void OnFixedUpdate() { }
        ///<summary>Called when state is updated</summary>
        ///<param name="dt">Time since last update of state machine</param>
        protected virtual void OnUpdate(float dt) { }
        ///<summary>Called when state is exited</summary>
        protected virtual void OnExit() { }

        ///<summary>Enters state then enters all valid children</summary>
        public void Enter() {
            if (Parent != null) { Parent.ActiveChild = this; }
            OnEnter();
            State initial = GetInitialState();
            if (initial != null) { initial.Enter(); }
        }

        ///<summary>Checks for transitions or updates children then self</summary>
        ///<param name="deltaTime">Time since last update of state machine</param>
        public void Update(float deltaTime) {
            State transition = GetTransition();
            if (transition != null) {
                StateMachine.ChangeState(this, transition);
                return;
            }

            if (ActiveChild != null) { ActiveChild.Update(deltaTime); }
            OnUpdate(deltaTime);
        }

        ///<summary>Calls OnFixedUpdate on children then self</summary>
        public void FixedUpdate() {
            if (ActiveChild != null) { ActiveChild.FixedUpdate(); }
            OnFixedUpdate();
        }

        ///<summary>Exits all children then exits self</summary>
        public void Exit() {
            if (ActiveChild != null) { ActiveChild.Exit(); } // Depth first out
            ActiveChild = null;
            OnExit();
        }

        ///<summary>Gets current active leaf node</summary>
        ///<returns>Leaf state or self</returns>
        public State GetLeaf() {
            State leaf = this;
            while (leaf.ActiveChild != null) { leaf = leaf.ActiveChild; }
            return leaf;
        }

        ///<summary>Find path to root state from current using parent</summary>
        ///<returns>List of states in order self -> parent -> ... -> root</returns>
        public IEnumerable<State> PathToRoot() {
            for (State current = this; current != null; current = current.Parent) {
                yield return current;
            }
        }


        ///<summary>Path to root from current state</summary>
        ///<param name="state">State to move to root from</param>
        ///<returns>Path to root (does not visit active children first)</returns>
        public static string StatePath(State state) {
            return string.Join(" > ", state.PathToRoot().Reverse().Select(state => state.GetType().Name));
        }

        ///<summary>Cast state up to real type - often used to get implementation specific data from parent</summary>
        ///<returns>State casted to type T</returns>
        public T Cast<T>() where T : State {
            return this as T;
        }

        ///<summary>Lowest Common Ancestor (parent in tree) between a and b</summary>
        ///<param name="a">First state<param>
        ///<param name="b">Second state<param>
        ///<returns>Common parent between two states or null<returns>
        public static State LCA(State a, State b) {
            HashSet<State> aParents = new HashSet<State>();
            for (State s = a; s != null; s = s.Parent) { aParents.Add(s); }
            for (State s = b; s != null; s = s.Parent) {
                if (aParents.Contains(s)) { return s; } // Parent of a and b
            }

            return null;
        }
    }
}