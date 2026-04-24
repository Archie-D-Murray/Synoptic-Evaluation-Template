using System.Collections.Generic;

using AI.Adapters;
using AI.HSM;

using UnityEngine;

namespace AI.Injectors {
    ///<summary>Provides an attacks and range checking logic
    public class AttackInjector : MonoBehaviour, IAttackInjector {

        ///<summary>Attacks an entity can do</summary>
        [SerializeReference, SubclassSelector]
        private List<AttackAdaptor> _attacks = new List<AttackAdaptor>() { new NullAttackAdapter() };

        ///<summary>Attack range for all entities using injector</summary>
        [SerializeField] private float _attackRange;

        ///<summary>Attack cooldown - probably should be replaced with something else</summary>
        [SerializeField] private float _attackTime = 1.0f;

        ///<summary>Name of attack cooldown</summary>
        [SerializeField] private string _attackCooldown = "Attack";

        ///<summary>Attack cooldown ID</summary>
        private int _attackCooldownID;

        ///<summary>Guard for first time initialisation</summary>
        private bool _initialised = false;

        ///<summary>Gets attacks for entities using injector</summary>
        ///<param name="context">Entity context</param>
        ///<returns>List of attack adapters to be queued into attack state</returns>
        public List<AttackAdaptor> GetAttacks(StateMachineContext context) {
            return _attacks;
        }

        ///<summary>Is attack timer finished</summary>
        ///<param name="context">Entity context</param>
        ///<returns>IsFinished value of attack timer cooldown</returns>
        public bool CanAttack(StateMachineContext context) {
            return context.CooldownManager.Get(_attackCooldownID).IsFinished;
        }

        ///<summary>OnEnter call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnEnter(StateMachineContext context) { }

        ///<summary>OnExit call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnExit(StateMachineContext context) { }

        ///<summary>OnUpdate call propagated from state - updates attack tiemr</summary>
        ///<param name="context">Entity context</param>
        ///<param name="dt">Time since last state machine update</param>
        public void OnUpdate(StateMachineContext context, float dt) {
            context.CooldownManager.Get(_attackCooldownID).Update(dt);
        }

        ///<summary>Used for first time initialisation</summary>
        public void Init() {
            if (_initialised) {
                return;
            }

            _attackCooldownID = AICooldownManager.GetHash(_attackCooldown);
            _initialised = true;
        }

        ///<summary>Gets attack time of entity associated with context</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Attack Time of entity associated with context</returns>
        public float AttackTime(StateMachineContext context) {
            return context.CooldownManager.Get(_attackCooldownID).InitialTime;
        }

        ///<summary>Gets attack time of entity associated with context</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Attack Time of entity associated with context</returns>
        public float AttackRange(StateMachineContext context) {
            return _attackRange;
        }

        ///<summary>Restarts attack cooldown as attack has occurred</summary>
        ///<param name="context">Entity context</param>
        public void RestartAttackCooldown(StateMachineContext context) {
            context.CooldownManager.Get(_attackCooldownID).Reset();
            context.CooldownManager.Get(_attackCooldownID).Start();
        }

        ///<summary>Is the target too far to attack or no target found</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Signal to transition back to different state</returns>
        public bool UnableToAttack(StateMachineContext context) {
            return !context.Detector.HasTarget() || !context.Detector.TargetPosition.InRange(context.Position, _attackRange);
        }

        ///<summary>Used for first initialisation per object using the injector</summary>
        ///<param name="context">Entity context</param>
        public void ContextInit(StateMachineContext context) {
            _attackCooldownID = context.CooldownManager.CreateCooldown(_attackTime, _attackCooldown);
        }
    }
}