using System;
using System.Collections.Generic;

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

        [SerializeReference, SubclassSelector] private IStateDefinition _definition = new DefaultStateDefinitions();
        [SerializeField] private AIStateView[] _states = new AIStateView[] { new AIStateView() { Key = AIState.Root, Parent = AIState.None } };
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
    }

    public interface IStateDefinition {
        public void InitFactory(StateFactory factory) { }
        public void InitInjectors(StateMachineContext context);
        public void InitTransitions(StateMachineContext context);
    }

    [Serializable]
    public class DefaultStateDefinitions : IStateDefinition {
        public void InitInjectors(StateMachineContext ctx) {
            ctx.IdleInjector = ctx.GetComponentInChildren<IIdleInjector>();
            ctx.WanderInjector = ctx.GetComponentInChildren<IWanderInjector>();
            ctx.PatrolInjector = ctx.GetComponentInChildren<IPatrolInjector>();
            ctx.ChaseInjector = ctx.GetComponentInChildren<IChaseInjector>();
            ctx.AttackInjector = ctx.GetComponentInChildren<IAttackInjector>();
            ctx.IdleInjector.Init();
            ctx.WanderInjector.Init();
            ctx.PatrolInjector.Init();
            ctx.ChaseInjector.Init();
            ctx.AttackInjector.Init();
            ctx.IdleInjector.ContextInit(ctx);
            ctx.WanderInjector.ContextInit(ctx);
            ctx.PatrolInjector.ContextInit(ctx);
            ctx.ChaseInjector.ContextInit(ctx);
            ctx.AttackInjector.ContextInit(ctx);
        }

        public void InitTransitions(StateMachineContext ctx) {
            ctx.StateMachine.AddInitialState(ctx[AIState.Root], ctx[AIState.Idle]);
            // Idle
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Wander],
                new AndPredicate(new LambdaPredicate(() => ctx.IdleInjector.DoneIdling(ctx)), new RandomChancePredicate(0.5f)));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Patrol],
                new AndPredicate(new LambdaPredicate(() => ctx.IdleInjector.DoneIdling(ctx)), new RandomChancePredicate(0.5f)));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Chase],
                new LambdaPredicate(ctx.Detector.HasTarget));

            // Wander
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Wander],
                ctx[AIState.Chase],
                new LambdaPredicate(ctx.Detector.HasTarget));

            // Patrol
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Patrol],
                ctx[AIState.Chase],
                new LambdaPredicate(ctx.Detector.HasTarget));

            // Chase
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Chase],
                ctx[AIState.Attack],
                new LambdaPredicate(() => ctx.ChaseInjector.InAttackRange(ctx, ctx.AttackInjector.AttackRange(ctx))));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Chase],
                ctx[AIState.Idle],
                new LambdaPredicate(() => ctx.ChaseInjector.LostTarget(ctx)));

            // Attack
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Attack],
                ctx[AIState.Chase],
                new LambdaPredicate(() => ctx.AttackInjector.UnableToAttack(ctx)));

        }
    }

    [Serializable]
    public class StationaryEnemyDefinition : IStateDefinition {

        public void InitInjectors(StateMachineContext ctx) {
            ctx.IdleInjector = ctx.GetComponentInChildren<IIdleInjector>();
            ctx.AttackInjector = ctx.GetComponentInChildren<IAttackInjector>();
            ctx.IdleInjector.Init();
            ctx.AttackInjector.Init();
            ctx.IdleInjector.ContextInit(ctx);
            ctx.AttackInjector.ContextInit(ctx);
        }

        public void InitTransitions(StateMachineContext ctx) {
            ctx.StateMachine.AddInitialState(ctx[AIState.Root], ctx[AIState.Idle]);
            // Idle
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Attack],
                new LambdaPredicate(() => ctx.Detector.HasTarget()));

            // Attack
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Attack],
                ctx[AIState.Idle],
                new LambdaPredicate(() => ctx.Detector.JustLostTarget));

        }
    }
}
