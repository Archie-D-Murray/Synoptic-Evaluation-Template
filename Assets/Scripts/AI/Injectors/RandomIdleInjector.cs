using AI.HSM;

using UnityEngine;

namespace AI.Injectors {

    ///<summary>Idles for random range before signalling for state change</summary>
    public class RandomIdleInjector : MonoBehaviour, IIdleInjector {

        ///<summary>Min idle time before done in seconds</summary>
        [SerializeField] private float _minIdleTime = 5;

        ///<summary>Max idle time before done in seconds</summary>
        [SerializeField] private float _maxIdleTime = 15;

        ///<summary>Idle cooldown ID</summary>
        private static int _waitTimeID = AICooldownManager.GetHash("WaitTime");

        ///<summary>Guard for first time initialisation</summary>
        private bool _initialised = false;

        ///<summary>Used for first time initialisation</summary>
        public void Init() {
            if (_initialised) {
                return;
            }

            _initialised = true;
        }

        ///<summary>OnEnter call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnEnter(StateMachineContext context) {
            context.CooldownManager.Get(_waitTimeID).Reset(Random.Range(_minIdleTime, _maxIdleTime));
            context.CooldownManager.Get(_waitTimeID).Start();
        }

        ///<summary>Signals if entity is finished idling</summary>
        ///<param name="context">Entity context</param>
        ///<returns>True if finished idling</returns>
        public bool DoneIdling(StateMachineContext context) {
            return context.CooldownManager.Get(_waitTimeID).IsFinished;
        }

        ///<summary>OnUpdate call propagated from state - ticks idle timer</summary>
        ///<param name="context">Entity context</param>
        ///<param name="dt">Time since last state machine update</param>
        public void OnUpdate(StateMachineContext context, float dt) {
            context.CooldownManager.Get(_waitTimeID).Update(dt);
        }

        ///<summary>OnExit call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnExit(StateMachineContext context) { }

        ///<summary>Used for first initialisation per object using the injector</summary>
        ///<param name="context">Entity context</param>
        public void ContextInit(StateMachineContext context) { }
    }
}