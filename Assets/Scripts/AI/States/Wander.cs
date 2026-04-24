using AI.HSM;

namespace AI {
    ///<summary>Randomly wanders to point in range of origin position</summary>
    public class WanderState : State {

        protected readonly StateMachineContext _context;

        public WanderState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
            _context = context;
        }

        ///<summary>Sets new wander point destination + propagates OnExit to injector</summary>
        protected override void OnEnter() {
            _context.WanderInjector.OnEnter(_context);
            _context.Movement.SetDestination(_context.WanderInjector.GetWanderPoint(_context));
        }

        ///<summary>Update Wander state handling ticking wander index and updating target destination</summary>
        ///<param name="dt"> Time since last update - used to update wander timer</param>
        protected override void OnUpdate(float dt) {
            _context.WanderInjector.OnUpdate(_context, dt);
            if (_context.WanderInjector.NextWanderPoint(_context)) {
                _context.Movement.SetDestination(_context.WanderInjector.GetWanderPoint(_context));
            }
            _context.Animator.SetFloat(Adapters.AIAnimationParam.Speed, _context.Movement.NormalizedSpeed);
        }

        ///<summary>Stops any movement from move adaptor + propagates OnExit to injector</summary>
        protected override void OnExit() {
            _context.WanderInjector.OnExit(_context);
            _context.Movement.SetDestination(_context.Position);
        }
    }
}