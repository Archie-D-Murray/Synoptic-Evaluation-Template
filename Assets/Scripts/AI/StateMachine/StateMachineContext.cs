using System;
using System.Collections.Generic;
using System.Linq;

using AI.Adapters;
using AI.Injectors;

using UnityEngine;

using Utilities;

namespace AI.HSM {

    [Serializable]
    public class AIStateView {
        public AIState Key;
        public AIState Parent;
        [HideInInspector]
        public State State = null;

        public AIStateView(AIState state, AIState parent) {
            Key = state;
            Parent = parent;
            State = null;
        }
    }

    class StateNode {
        public StateNode Parent;
        public State Value;
        public AIState Type;
        public List<StateNode> Children;
    }

    ///<summary>Base context for the state machine</summary>
    ///<summary>Add references to things like an enemy script here to use in states/injectors</summary>
    public sealed class StateMachineContext : MonoBehaviour {

        /// ------------------------------ \\\
        /// Data members added by examples \\\
        /// ------------------------------ \\\
        public float AttackRange = 5.0f;
        public float RangedRange = 7.5f;
        public float RangedMovementLockout = 0.5f;

        public IIdleInjector IdleInjector;
        public IWanderInjector WanderInjector;
        public IPatrolInjector PatrolInjector;
        public IChaseInjector ChaseInjector;
        public IAttackInjector AttackInjector;
        /// --------------------------- \\\
        /// Injectors added by examples \\\
        /// --------------------------- \\\
        public IAttackInjector RangedInjector;

        public MovementAdapter Movement;
        public AnimationAdapter Animator;
        public StateMachine StateMachine;
        public DetectorAdapter Detector;
        public AttackContext AttackContext;
        public AICooldownManager CooldownManager = new AICooldownManager(new AICooldown[] {
            new AICooldown("WaitTime", 1.0f, false),
            new AICooldown("WanderTimer", 4.0f, false),
            new AICooldown("TimePerPoint", 2.0f, false),
            new AICooldown("LostTargetTimer", 3.0f, false),
            new AICooldown("Attack", 1.0f, false),
        });
        public Vector3 Position => transform.position;

        [SerializeReference, SubclassSelector] private IStateDefinition _definition = StateMachineContext.GetDefault();
        [SerializeField] private AIStateView[] _states = new AIStateView[] { new AIStateView(AIState.Root, AIState.None) };
        private Dictionary<AIState, int> _lookup = new Dictionary<AIState, int>();

        ///<summary>Gets a state object from state type or null if not present</summary>
        private State Get(AIState state) {
            if (_lookup.TryGetValue(state, out int index)) {
                return _states[index].State;
            } else {
                return null;
            }
        }

        ///<summary>Gets a state object from state type or null if not present</summary>
        public State this[AIState state] {
            get {
                return Get(state);
            }
            private set {
                if (_lookup.TryGetValue(state, out int index)) {
                    _states[index].State = value;
                }
            }
        }

        private void OnValidate() {
            Animator = GetComponentInChildren<AnimationAdapter>();
            Movement = GetComponentInChildren<MovementAdapter>();
            Detector = GetComponentInChildren<DetectorAdapter>();
            if (AttackContext == null) {
                AttackContext = new AttackContext(this);
            } else {
                AttackContext.Entity = this;
            }

            if (_definition != null) {
                HashSet<AIState> tempLookup = new HashSet<AIState>(_states.Length);
                foreach (AIStateView view in _states) {
                    tempLookup.Add(view.Key);
                }

                int oldSize = _states.Count();
                IEnumerable<AIState> toAdd = _definition.RequiredStates().Where(state => !tempLookup.Contains(state));
                Array.Resize(ref _states, _states.Length + toAdd.Count());
                for (int i = oldSize; i < _states.Length; i++) {
                    if (toAdd.ElementAt(i - oldSize) == AIState.Root) {
                        _states[i] = new AIStateView(AIState.Root, AIState.None);
                    } else {
                        _states[i] = new AIStateView(toAdd.ElementAt(i - oldSize), AIState.Root);
                    }
                }
            }
        }

        private void Awake() {
            Init();
        }

        ///<summary>Initialises states as dependency tree</summary>
        private void Init() {
            int i = 0;

            StateMachine = new StateMachine();
            StateFactory factory = new StateFactory(StateMachine, this);

            _definition.InitFactory(factory);

            StateMachine.SetRoot(factory.Create(AIState.Root, null));

            foreach (AIStateView view in _states) {
                _lookup.Add(view.Key, i);
                i++;
            }
            Dictionary<AIState, int> visited = new Dictionary<AIState, int>();

            foreach (AIStateView view in _states) {
                if (view.Key == AIState.Root) { continue; }
                InitState(view, factory, visited);
            }

            if (_definition != null) {
                _definition.InitInjectors(this);
                _definition.InitTransitions(this);
            } else {
                Helpers.ContextLog(this, "State machine state defitions are null, this is not valid, please select an option from the drop down...");
                enabled = false;
                return;
            }
        }

        ///<summary>Initialises view.State with real state from factory resolving parents first</summary>
        ///<remarks>Root state should already be initialised</remarks>
        private void InitState(AIStateView view, StateFactory factory, Dictionary<AIState, int> visited) {
            if (view.Key == AIState.None) {
                return;
            }
            if (visited.GetValueOrDefault(view.Key, 0) > 1) {
                Debug.LogError($"Visited state {view.State} more than once - is there a cyclical dependency?");
                return;
            }
            if (view.Key == AIState.Root) {
                view.State = StateMachine.Root;
                return;
            }
            if (this[view.Parent] == null) {
                InitState(_states[_lookup[view.Parent]], factory, visited);
            }
            if (visited.ContainsKey(view.Key)) {
                visited[view.Key]++;
            } else {
                visited.Add(view.Key, 1);
            }
            this[view.Key] = factory.Create(view.Key, this[view.Parent]);
        }

        ///<summary>Updates state machine</summary>
        ///<summary>Unlikely to change unless state machine needs to not tick on Update</summary>
        private void Update() {
            float dt = Time.deltaTime;
            CooldownManager.Update(dt);
            StateMachine.Tick(dt);
        }

        ///<summary>Forwards FixedUpdate to state machine</summary>
        private void FixedUpdate() {
            StateMachine.FixedTick();
        }
        
        private static IStateDefinition GetDefault() {
#if AI_EXAMPLES
            return new AI.Examples.RangedStateDefinition();
#else
            return new DefaultStateDefinitions();
#endif
        }
    }
}