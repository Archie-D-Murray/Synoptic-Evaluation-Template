using AI.HSM;

namespace AI {

    ///<summary>A blank state to stop any transitions moving out of the current state</summary>
    [System.Serializable]
    public class DeadState : State {

        protected readonly StateMachineContext _context;

        public DeadState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
            _context = context;
        }

        ///<summary>Plays death animation if present</summary>
        protected override void OnEnter() {
            _context.Animator.Play(Adapters.AIAnimationType.Dead);
        }

        protected override void OnUpdate(float dt) {
            _context.Animator.SetFloat(Adapters.AIAnimationParam.Speed, 0.0f);
        }
    }
}