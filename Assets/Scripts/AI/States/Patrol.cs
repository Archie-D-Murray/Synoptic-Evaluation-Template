using AI.Adapters;
using AI.HSM;

namespace AI {

    ///<summary>Moves along a set of patrol points waiting for a specified duration at each point</summary>
    [System.Serializable]
    public class PatrolState : State {

        protected readonly StateMachineContext _context;
        [UnityEngine.SerializeField] protected int _patrolIndex = 0;

        public PatrolState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
            _context = context;
        }

        ///<summary>Sets destination to patrol point + propagates OnEnter to patrol injector</summary>
        ///<param name="dt"> Time since last update - used to update patrol point timer</param>
        protected override void OnEnter() {
            _patrolIndex = _context.PatrolInjector.GetStartIndex(_context, _patrolIndex);
            _context.PatrolInjector.OnEnter(_context);
            _context.Movement.SetDestination(_context.PatrolInjector.GetPatrolTarget(_context, _patrolIndex));
        }

        ///<summary>Update Patrol state handling ticking current patrol index and updating target destination</summary>
        ///<param name="dt">Time since last update - used to update patrol point timer</param>
        protected override void OnUpdate(float dt) {
            _context.PatrolInjector.OnUpdate(_context, dt);

            // At current patrol target
            if (_context.PatrolInjector.AtPatrolPoint(_context, _context.Position, _patrolIndex)) {
                _context.PatrolInjector.TickPatrolPoint(_context, dt);
            }

            // Move to next patrol point
            if (_context.PatrolInjector.FinishedPatrolPoint(_context, _patrolIndex)) {
                _context.PatrolInjector.OnPatrolPointFinish(_context);
                _patrolIndex = _context.PatrolInjector.Next(_context, _patrolIndex);
                _context.Movement.SetDestination(_context.PatrolInjector.GetPatrolTarget(_context, _patrolIndex));
            }
            _context.Animator.SetFloat(AIAnimationParam.Speed, _context.Movement.NormalizedSpeed);
        }

        ///<summary>Stops any movement from move adaptor + propagates OnExit to injector</summary>
        protected override void OnExit() {
            _context.PatrolInjector.OnExit(_context);
            _context.Movement.SetDestination(_context.Position);
        }
    }
}