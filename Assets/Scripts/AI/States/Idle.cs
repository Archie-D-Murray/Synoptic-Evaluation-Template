using AI.HSM;

namespace AI {
    ///<summary>Idles for a set time before signalling for a state change</summary>
    [System.Serializable]
    public class IdleState : State {

        protected readonly StateMachineContext _context;

        public IdleState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
            _context = context;
        }

        ///<summary>Propagates OnEnter to injector</summary>
        protected override void OnEnter() {
            _context.IdleInjector.OnEnter(_context);
        }

        ///<summary>Update Idle state + propagates OnUpdate to Idle Injector</summary>
        ///<param name="dt"> Time since last update - used to update idle timer</param>
        protected override void OnUpdate(float dt) {
            _context.IdleInjector.OnUpdate(_context, dt);
            _context.Animator.SetFloat(Adapters.AIAnimationParam.Speed, _context.Movement.NormalizedSpeed);
        }

        ///<summary>Propagates OnExit to injector</summary>
        protected override void OnExit() {
            _context.IdleInjector.OnExit(_context);
        }
    }
}