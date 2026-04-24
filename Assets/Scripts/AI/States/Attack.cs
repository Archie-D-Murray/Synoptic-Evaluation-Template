using Utilities;

using AI.Adapters;
using AI.HSM;

namespace AI {
    ///<summary>Used for attacking enemies that are in range</summary>
    [System.Serializable]
    public class AttackState : State {

        protected readonly StateMachineContext _context;
        protected readonly PriorityQueue<AttackAdaptor, float> _attacks = new PriorityQueue<AttackAdaptor, float>(16);
        protected bool _isAttacking = false;
        protected float _elapsedAttackTime = 0.0f;

        public AttackState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
            _context = context;
        }

        ///<summary>Propagates OnEnter to injector</summary>
        protected override void OnEnter() {
            _context.AttackInjector.OnEnter(_context);
            _context.Animator.Play(AIAnimationType.Attack);

            _context.AttackContext.State = this;
            _context.AttackContext.Origin = _context.Position.Offset(y: 1.5f);
            _context.AttackContext.Direction = (_context.Detector.TargetPosition - _context.Position).normalized;
        }

        ///<summary>Update Attack state handling attacking and ticking pending attacks</summary>
        ///<param name="dt">Time since last update - used to update attack duration and queued attacks</param>
        protected override void OnUpdate(float dt) {
            _context.AttackInjector.OnUpdate(_context, dt);

            // Can this entity attack?
            if (_context.AttackInjector.CanAttack(_context)) {
                _attacks.Clear();
                _isAttacking = true;
                _context.Movement.SetDestination(_context.Position);
                _context.AttackInjector.RestartAttackCooldown(_context);
                _elapsedAttackTime = 0.0f;
                _context.AttackContext.Clip = _context.Animator.GetCurrentClip();
                foreach (AttackAdaptor attack in _context.AttackInjector.GetAttacks(_context)) {
                    // Attacks are ordered by normalized time
                    if (attack.NormalizedTime == 0.0f) {
                        attack.OnEvent(_context.AttackContext);
                    } else {
                        _attacks.Enqueue(attack, attack.NormalizedTime);
                    }
                }
            }

            // Handle any pending attacks
            if (_attacks.Count > 0) {
                _elapsedAttackTime += dt;
                float normalized = _elapsedAttackTime / _context.AttackInjector.AttackTime(_context);
                _context.AttackContext.Clip = _context.Animator.GetCurrentClip();
                while (_attacks.Count > 0 && _attacks.Peek().NormalizedTime <= normalized) {
                    _attacks.Dequeue().OnEvent(_context.AttackContext);
                }

            }
            if (_elapsedAttackTime >= _context.AttackInjector.AttackTime(_context)) {
                _isAttacking = false;
            }
            _context.Animator.SetFloat(Adapters.AIAnimationParam.Speed, _context.Movement.NormalizedSpeed);
        }

        ///<summary>Stops any movement from move adaptor + propagates OnExit to injector</summary>
        protected override void OnExit() {
            _context.AttackInjector.OnExit(_context);
            _context.Animator.Play(AIAnimationType.Locomotion);
        }
    }
}