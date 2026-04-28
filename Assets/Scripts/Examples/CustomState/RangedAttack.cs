using System;

using AI.Adapters;
using AI.HSM;

using UnityEngine;

using Utilities;

namespace AI.Examples {

    ///<summary>Used for attacking enemies that are in range while not in melee range</summary>
    ///<summary>Will start moving towards its target once close enough</summary>
    [System.Serializable]
    public class RangedState : State {

        protected readonly StateMachineContext _context;
        protected readonly PriorityQueue<AttackAdaptor, float> _attacks = new PriorityQueue<AttackAdaptor, float>(16);
        protected bool _isAttacking = false;
        protected float _elapsedAttackTime = 0.0f;

        public RangedState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
            _context = context;
        }

        ///<summary>Propagates OnEnter to injector and sets attack context</summary>
        protected override void OnEnter() {
            _context.RangedInjector.OnEnter(_context);

            _context.AttackContext.State = this;
            _context.AttackContext.Origin = _context.Position.Offset(y: 1.5f);
            _context.AttackContext.Direction = (_context.Detector.TargetPosition - _context.Position).normalized;
            _elapsedAttackTime = 0.0f;
        }

        ///<summary>Update state handling attacking and ticking pending attacks</summary>
        ///<summary>Additionally reenables movement after a lockout period</summary>
        ///<param name="dt">Time since last update - used to update attack duration and queued attacks</param>
        protected override void OnUpdate(float dt) {
            _context.AttackContext.Origin = _context.Position.Offset(y: 1.5f);
            _context.AttackContext.Direction = (_context.Detector.TargetPosition - _context.Position).normalized;

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

        ///<summary>Moves back to moving animation + propagates OnExit to injector</summary>
        protected override void OnExit() {
            _elapsedAttackTime = 0.0f;
            _context.RangedInjector.OnExit(_context);
            _context.Animator.Play(AIAnimationType.Locomotion);
        }

        ///<summary>Updates target destination if necessary</summary>
        private void TryUpdateDestination() {
            if (!_context.Movement.PathPending) {
                _context.Movement.SetDestination(_context.Detector.TargetPosition);
            }
        }
    }

    ///<summary>Attack Adaptor respresenting a ranged attack</summary>
    [Serializable]
    public class RangedAttackAdaptor : AttackAdaptor {

        ///<summary>Projectile to spawn</summary>
        [SerializeField] private RangedProjectile _projectile;

        ///<summary>Valid object layers to damage</summary>
        [SerializeField] private LayerMask _mask;

        ///<summary>Damage amount</summary>
        [SerializeField] private float _damage = 4.0f;

        ///<summary>Projectile lifetime</summary>
        [SerializeField] private float _lifeTime = 5.0f;

        ///<summary>Projectile speed</summary>
        [SerializeField] private float _speed = 5.0f;

        ///<summary>Attack Adaptor respresenting a ranged attack</summary>
        ///<param name="context">Attack context to initialse attack parameters with</param>
        public override void OnEvent(AttackContext context) {
            UnityEngine.Object.Instantiate<RangedProjectile>(_projectile, context.Origin + context.Direction * 0.5f, Quaternion.identity)
                .Fire(context.Direction, _lifeTime, _speed, _damage, context.Entity.gameObject);
        }
    }
}