using System;

using AI.Adapters;
using AI.HSM;

using UnityEngine;

using Utilities;

namespace AI.Examples {

    [System.Serializable]
    public class RangedState : State {

        protected readonly StateMachineContext _context;
        protected readonly PriorityQueue<AttackAdaptor, float> _attacks = new PriorityQueue<AttackAdaptor, float>(16);
        protected bool _isAttacking = false;
        protected float _elapsedAttackTime = 0.0f;

        public RangedState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
            _context = context;
        }

        protected override void OnEnter() {
            _context.RangedInjector.OnEnter(_context);

            _context.AttackContext.State = this;
            _context.AttackContext.Origin = _context.Position.Offset(y: 1.5f);
            _context.AttackContext.Direction = (_context.Detector.TargetPosition - _context.Position).normalized;
        }

        protected override void OnUpdate(float dt) {

            _context.RangedInjector.OnUpdate(_context, dt);

            // Can this entity attack?
            if (_context.RangedInjector.CanAttack(_context)) {
                _attacks.Clear();
                _isAttacking = true;
                _context.Animator.Play(AIAnimationType.Ranged);
                _context.RangedInjector.RestartAttackCooldown(_context);
                _context.Movement.SetDestination(_context.Position);
                _elapsedAttackTime = 0.0f;
                _context.AttackContext.Clip = _context.Animator.GetCurrentClip();
                foreach (AttackAdaptor attack in _context.RangedInjector.GetAttacks(_context)) {
                    // Attacks are ordered by normalized time
                    if (attack.NormalizedTime == 0.0f) {
                        attack.OnEvent(_context.AttackContext);
                    } else {
                        _attacks.Enqueue(attack, attack.NormalizedTime);
                    }
                }
            }

            float normalizedAttackTime = _elapsedAttackTime / _context.RangedInjector.AttackTime(_context);
            if (_isAttacking) {
                // Handle any pending attacks
                _elapsedAttackTime += dt;
                if (_attacks.Count > 0) {
                    _context.AttackContext.Clip = _context.Animator.GetCurrentClip();
                    while (_attacks.Count > 0 && _attacks.Peek().NormalizedTime <= normalizedAttackTime) {
                        _attacks.Dequeue().OnEvent(_context.AttackContext);
                    }

                }
                if (_isAttacking && normalizedAttackTime >= 1.0f) {
                    _isAttacking = false;
                    _context.Animator.Play(AIAnimationType.Locomotion);
                }
            }

            if (normalizedAttackTime >= _context.RangedMovementLockout || !_isAttacking) {
                _context.Animator.Play(AIAnimationType.Locomotion);
                TryUpdateDestination();
            }
            _context.Animator.SetFloat(Adapters.AIAnimationParam.Speed, _context.Movement.NormalizedSpeed);
        }

        protected override void OnExit() {
            _context.RangedInjector.OnExit(_context);
            _context.Animator.Play(AIAnimationType.Locomotion);
        }

        private void TryUpdateDestination() {
            if (!_context.Movement.PathPending && Vector3.Distance(_context.Movement.Target, _context.Detector.TargetPosition) <= 0.1f) {
                _context.Movement.SetDestination(_context.Detector.TargetPosition);
            }
        }
    }

    [Serializable]
    public class RangedAttackAdaptor : AttackAdaptor {

        [SerializeField] private RangedProjectile _projectile;
        [SerializeField] private LayerMask _mask;
        [SerializeField] private float _damage = 4.0f;
        [SerializeField] private float _lifeTime = 5.0f;
        [SerializeField] private float _speed = 5.0f;

        public override void OnEvent(AttackContext context) {
            UnityEngine.Object.Instantiate<RangedProjectile>(_projectile, context.Origin + context.Direction * 0.5f, Quaternion.identity)
                .Fire(context.Direction, _lifeTime, _speed, _damage, context.Entity.gameObject);
        }
    }
}
