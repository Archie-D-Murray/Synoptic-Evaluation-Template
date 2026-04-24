using AI.HSM;

using UnityEngine;

namespace AI.Injectors {
    ///<summary>Moves toward target switching to attack if in range, or giving up if lost target and not found within window of time
    public class ChaseInjector : MonoBehaviour, IChaseInjector {

        ///<summary>Chase range before signaling to give up chase</summary>
        [SerializeField] private float _chaseRange;

        private bool _initialised = false;

        ///<summary>Attack cooldown ID</summary>
        private static int _lostTargetID = AICooldownManager.GetHash("LostTargetTimer");

        ///<summary>Completely lost target and needs to give up chasing</summary>
        ///<param name="context">Entity context</param>
        ///<returns>No valid target and did not find new target within time limit</returns>
        public bool LostTarget(StateMachineContext context) {
            return !context.Detector.Target && context.CooldownManager.Get(_lostTargetID).IsFinished;
        }

        ///<summary>OnEnter call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnEnter(StateMachineContext context) { }

        ///<summary>OnExit call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnExit(StateMachineContext context) { }

        ///<summary>OnUpdate call propagated from state - updates lost target timer</summary>
        ///<param name="context">Entity context</param>
        ///<param name="dt">Time since last state machine update</param>
        public void OnUpdate(StateMachineContext context, float dt) {
            context.CooldownManager.Get(_lostTargetID).Update(dt);
        }

        ///<summary>Used for first time initialisation</summary>
        public void Init() {
            if (_initialised) {
                return;
            }

            _initialised = true;
        }

        ///<summary>Starts window to resume chase upon finding new target</summary>
        ///<param name="context">Entity context</param>
        public void StartLostTimer(StateMachineContext context) {
            context.CooldownManager.Get(_lostTargetID).Start();
        }

        ///<summary>Whether entity is in range to start attacking</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Target is within attack range</returns>
        public bool InAttackRange(StateMachineContext context, float attackRange) {
            return context.Detector.TargetPosition.InRange(context.Position, attackRange) && context.Detector.HasTarget();
        }

        ///<summary>Used for first initialisation per object using the injector</summary>
        ///<param name="context">Entity context</param>
        public void ContextInit(StateMachineContext context) { }
    }
}