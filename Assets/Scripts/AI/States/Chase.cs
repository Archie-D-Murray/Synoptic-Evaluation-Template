using AI.HSM;

using UnityEngine;

namespace AI {
    ///<summary>Used for chasing an enemy when are still seen but out of range to attack</summary>
    [System.Serializable]
    public class ChaseState : State {

        protected readonly StateMachineContext _context;
        protected Transform _target;

        public ChaseState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
            _context = context;
        }

        ///<summary>Propagates OnEnter to injector</summary>
        protected override void OnEnter() {
            _context.ChaseInjector.OnEnter(_context);
        }

        ///<summary>Update chase state with target checking + updating chase destination</summary>
        ///<param name="dt"> Time since last update - used to update lost target timer</param>
        protected override void OnUpdate(float dt) {
            _context.ChaseInjector.OnUpdate(_context, dt);

            _context.Movement.SetDestination(_context.Detector.TargetPosition);
            if (_context.Detector.JustLostTarget) {
                _context.ChaseInjector.StartLostTimer(_context);
            }

            _context.Animator.SetFloat(Adapters.AIAnimationParam.Speed, _context.Movement.NormalizedSpeed);
        }

        ///<summary>Propagates OnExit to injector</summary>
        ///<summary>Do not call SetDestination with current pos as it could interfere with attack state</summary>
        protected override void OnExit() {
            _context.ChaseInjector.OnExit(_context);
        }
    }
}