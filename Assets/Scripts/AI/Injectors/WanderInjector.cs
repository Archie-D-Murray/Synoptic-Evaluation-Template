using System.Collections.Generic;

using AI.HSM;

using UnityEngine;

namespace AI.Injectors {
    ///<summary>Randomly wanders to locations within range of reference and waits for random range of time</summary>
    public class WanderInjector : MonoBehaviour, IWanderInjector {

        ///<summary>Max wander distance</summary>
        [SerializeField] private float _maxRange = 10.0f;

        ///<summary>Min time to stay at wander point</summary>
        [SerializeField] private float _minWanderTime = 5.0f;

        ///<summary>Max time to stay at wander point</summary>
        [SerializeField] private float _maxWanderTime = 10.0f;

        ///<summary>Wander idle cooldown ID</summary>
        private int _wanderTimerID = AICooldownManager.GetHash("WanderTimer");

        ///<summary>Guard for first time initialisation</summary>
        private bool _initialised = false;

        ///<summary>Reference for wander points</summary>
        private Dictionary<StateMachineContext, Vector3> _initialPositions = new Dictionary<StateMachineContext, Vector3>();

        ///<summary>Gets a random wander point within specified ranges</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Target destination with y = 0</returns>
        public Vector3 GetWanderPoint(StateMachineContext context) {
            context.CooldownManager.Get(_wanderTimerID).Reset(Random.Range(_minWanderTime, _maxWanderTime));
            return _initialPositions[context] + (Random.insideUnitCircle * _maxRange).ToXZ();
        }

        ///<summary>Used for first time initialisation</summary>
        public void Init() {
            if (_initialised) {
                return;
            }

            _initialised = true;
        }

        ///<summary>OnEnter call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnEnter(StateMachineContext context) { }

        ///<summary>OnEnter call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnExit(StateMachineContext context) { }

        ///<summary>OnUpdate call propagated from state - updates wander timer if within 0.5 units of target</summary>
        ///<param name="context">Entity context</param>
        ///<param name="dt">Time since last state machine update</param>
        public void OnUpdate(StateMachineContext context, float dt) {
            if (Vector3.Distance(context.Movement.Target, context.Position) <= 0.5f && !context.CooldownManager.Get(_wanderTimerID).IsRunning) {
                context.CooldownManager.Get(_wanderTimerID).Update(dt);
            }
        }

        ///<summary>Returns if entity needs a new wander point as it has finished idling at it</summary>
        ///<param name="context">Entity context</param>
        ///<returns>True if new wander point is needed</returns>
        public bool NextWanderPoint(StateMachineContext context) {
            return context.CooldownManager.Get(_wanderTimerID).IsFinished;
        }

        ///<summary>Used for first initialisation per object using the injector</summary>
        ///<param name="context">Entity context</param>
        public void ContextInit(StateMachineContext context) {
            _initialPositions.Add(context, context.Position);
        }
    }
}