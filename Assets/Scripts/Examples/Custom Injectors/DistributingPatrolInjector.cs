using System;
using System.Collections.Generic;

using AI.HSM;
using AI.Injectors;

using UnityEngine;

namespace AI.Examples {

    ///<summary>Randomly wanders to locations within range of reference and waits for random range of time</summary>
    public class DistributedPatrolInjector : MonoBehaviour, IPatrolInjector {

        [Serializable]
        class Patrol {
            public List<StateMachineContext> PatrolMembers;

            public Patrol() {
                PatrolMembers = new List<StateMachineContext>(5);
            }

            public Patrol(int capacity) {
                PatrolMembers = new List<StateMachineContext>(capacity);
            }

            public StateMachineContext this[int index] {
                get => PatrolMembers[index];
                set => PatrolMembers[index] = value;
            }

            public int Count => PatrolMembers.Count;
        }

        private int _nextPatrolIndex = 0;

        [SerializeField] private bool _showPatrolPointsDebug = false;

        ///<summary>All patrol points, showing entities matching a current target</summary>
        [SerializeField] Patrol[] _currentPatrols;

        ///<summary>Mapping of state machine contexts to current patrol index</summary>
        Dictionary<StateMachineContext, int> _contextToPos;

        ///<summary>Patrol points</summary>
        [SerializeField] private Vector3[] _patrolPoints;

        ///<summary>Max distance from patrol point to be counted as at point</summary>
        [SerializeField] private float _targetDistance = 0.5f;

        ///<summary>Guard for first time initialisation</summary>
        private bool _initialised = false;

        ///<summary>Patrol wait timer cooldown ID</summary>
        private static int _timePerPointID = AICooldownManager.GetHash("TimePerPoint");

        ///<summary>Get starting patrol point</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>New patrol index</returns>
        public int GetStartIndex(StateMachineContext context, int index) {
            int newIndex = ++_nextPatrolIndex % _patrolPoints.Length;

            _contextToPos.Add(context, newIndex);
            _currentPatrols[newIndex].PatrolMembers.Add(context);

            return newIndex;
        }

        ///<summary>Get target patrol point</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>Target patrol destination</returns>
        public Vector3 GetPatrolTarget(StateMachineContext context, int index) {
            return _patrolPoints[index];
        }

        ///<summary>Used for first time initialisation</summary>
        public void Init() {
            if (_initialised) {
                return;
            }
            _currentPatrols = new Patrol[_patrolPoints.Length];
            for (int i = 0; i < _patrolPoints.Length; i++) {
                _currentPatrols[i] = new Patrol(5);
            }

            _contextToPos = new Dictionary<StateMachineContext, int>();

            _initialised = true;
        }

        ///<summary>OnEnter call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnEnter(StateMachineContext context) {
            context.CooldownManager.Get(_timePerPointID).Resume();
        }

        ///<summary>OnUpdate call propagated from state</summary>
        ///<param name="context">Entity context</param>
        ///<param name="dt">Time since last state machine update</param>
        public void OnUpdate(StateMachineContext context, float dt) { }

        ///<summary>OnExit call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnExit(StateMachineContext context) {
            int patrol = _contextToPos[context];
            _currentPatrols[patrol].PatrolMembers.Remove(context);
            _contextToPos.Remove(context);
        }

        ///<summary>Get next patrol index wrapped to patrol points length</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>Next patrol index</returns>
        public int Next(StateMachineContext context, int index) {
            int newIndex = (index + 1) % _patrolPoints.Length;

            _currentPatrols[index].PatrolMembers.Remove(context);
            _currentPatrols[newIndex].PatrolMembers.Add(context);
            _contextToPos[context] = newIndex;

            return newIndex;
        }

        ///<summary>Get previous patrol index wrapped to patrol points length</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>Previous patrol index</returns>
        public int Prev(StateMachineContext context, int index) {
            int newIndex = index > 1 ? index - 1 : _patrolPoints.Length - 1;

            _currentPatrols[index].PatrolMembers.Remove(context);
            _currentPatrols[newIndex].PatrolMembers.Add(context);
            _contextToPos[context] = newIndex;

            return newIndex;
        }

        ///<summary>Update wait timer when close enough to patrol point</summary>
        ///<param name="context">Entity context</param>
        ///<param name="dt">Time to tick timer by</param>
        public void TickPatrolPoint(StateMachineContext context, float dt) {
            context.CooldownManager.Get(_timePerPointID).Update(dt);
        }

        ///<summary>Used to find if entity needs to advance to new patrol point</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>Has finished idling at patrol point</returns>
        public bool FinishedPatrolPoint(StateMachineContext context, int index) {
            return context.CooldownManager.Get(_timePerPointID).IsFinished;
        }

        ///<summary>Called upon finishing idling at patrol point</summary>
        ///<param name="context">Entity context</param>
        public void OnPatrolPointFinish(StateMachineContext context) {
            context.CooldownManager.Get(_timePerPointID).Start();
        }

        ///<summary>Is close enough to patrol point to begin idling</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>True if close enough to patrol point</returns>
        public bool AtPatrolPoint(StateMachineContext context, Vector3 position, int index) {
            return Vector3.Distance(position, GetPatrolTarget(context, index)) <= _targetDistance;
        }

        ///<summary>Used for first initialisation per object using the injector</summary>
        ///<param name="context">Entity context</param>
        public void ContextInit(StateMachineContext context) { }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!_showPatrolPointsDebug) { return; }
            Color prev = Gizmos.color;
            Gizmos.color = Color.grey;
            foreach (Vector3 point in _patrolPoints) {
                Gizmos.DrawSphere(point, 0.5f);
            }

            Gizmos.color = prev;
        }
#endif
    }
}