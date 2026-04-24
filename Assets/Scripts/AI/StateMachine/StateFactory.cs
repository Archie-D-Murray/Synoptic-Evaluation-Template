using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI.HSM {

    using StateCreator = Func<StateMachineContext, StateMachine, State, State>;

    public class StateFactoryDefinition {
        public AIState StateType;
        public StateCreator CreationFunction;

        public StateFactoryDefinition(AIState type, StateCreator creator) {
            StateType = type;
            CreationFunction = creator;
        }
    }

    public class StateFactory {

        private StateMachine _stateMachine;
        private StateMachineContext _context;

        private Dictionary<AIState, StateCreator> _externalDefinitions;

        ///<summary>Add a custom state creator - will override default behaviour of state machine
        public void AddStateDefinitions(StateFactoryDefinition[] definitions) {
            foreach (StateFactoryDefinition definition in definitions) {
                _externalDefinitions.Add(definition.StateType, definition.CreationFunction);
            }
        }

        ///<summary>Add a custom state creator - will override default behaviour of state machine
        public void AddStateDefinition(StateFactoryDefinition definition) {
            _externalDefinitions.Add(definition.StateType, definition.CreationFunction);
        }

        ///<summary>Used for constructing states from state list in inspector</summary>
        ///<remarks>Only to be used with state machine with root already created</remarks>
        ///<param name="stateMachine">State machine to use in state initialisation</param>
        ///<param name="context">Context for state machine</param>
        public StateFactory(StateMachine stateMachine, StateMachineContext context) {
            _externalDefinitions = new Dictionary<AIState, StateCreator>();
            _stateMachine = stateMachine;
            _context = context;
        }

        ///<summary>Creates state of type with parent</summary>
        ///<param name="type">State type to construct as real State</param>
        ///<param name="parent">Parent of state - can be null</param>
        ///<returns>Newly created state</returns>
        public State Create(AIState type, State parent = null) {
            if (_externalDefinitions.ContainsKey(type)) {
                return _externalDefinitions[type](_context, _stateMachine, parent);
            }

            return type switch {
                AIState.Root => new RootState(_stateMachine, null),
                AIState.Idle => new IdleState(_context, _stateMachine, parent),
                AIState.Wander => new WanderState(_context, _stateMachine, parent),
                AIState.Patrol => new PatrolState(_context, _stateMachine, parent),
                AIState.Chase => new ChaseState(_context, _stateMachine, parent),
                AIState.Attack => new AttackState(_context, _stateMachine, parent),
                AIState.Dead => new DeadState(_context, _stateMachine, parent),
                _ => null
            };
        }

        ///<summary>Used for invalid Create calls as switch returns cannot log and return</summary>
        private T ReturnWithLog<T>(T value, string message) {
            Debug.LogError($"[StateFactory (Object: {_context.name})] {message}", _context);
            return value;
        }
    }
}