```cs
using AI.Injectors;
using AI.HSM;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Examples {

    [System.Serializable]
    public class Route {
        public Vector3[] Points = new Vector3[3];
        public int CurrentCount = 0;

        public Route(Transform root) {
            Points = new Vector3[root.childCount];

            for (int i = 0; i < root.childCount; i++) {
                Points[i] = root.GetChild(i).position;
            }

            CurrentCount = 0;
        }
    }

    public class AlternatingPatrolInjector : MonoBehaviour, IPatrolInjector {
        private Dictionary<StateMachineContext, int> _contextToRoute = new Dictionary<StateMachineContext, int>();

        [SerializeField] private Route[] _routes;
        [SerializeField] private float _targetDistance = 0.5f;
        [SerializeField] private Transform[] _roots;

        private static int _timePerPointID = AICooldownManager.GetHash("TimePerPoint");

        public bool AtPatrolPoint(StateMachineContext context, Vector3 position, int index) {
            return Vector3.Distance(position, GetPatrolTarget(context, index)) <= _targetDistance;
        }

        public void ContextInit(StateMachineContext context) { }

        public bool FinishedPatrolPoint(StateMachineContext context, int index) {
            return context.CooldownManager.Get(_timePerPointID).IsFinished;
        }

        public Vector3 GetPatrolTarget(StateMachineContext context, int index) {
            return _routes[_contextToRoute[context]].Points[index];
        }

        public int GetStartIndex(StateMachineContext context, int index) {
            int target = 0;
            int count = int.MaxValue;
            for (int i = 0; i < _routes.Length; i++) {
                if (_routes[i].CurrentCount < count) {
                    target = i;
                    count = _routes[i].CurrentCount;
                }
            }

            _contextToRoute.Add(context, target);

            _routes[target].CurrentCount++;

            return target;
        }

        public void Init() {
            int i = 0;
            _routes = new Route[_roots.Length];
            foreach (Transform root in _roots) {
                _routes[i] = new Route(root);
                i++;
            }
        }

        public int Next(StateMachineContext context, int index) {
            return (index + 1) % _routes[_contextToRoute[context]].Points.Length;
        }

        public void OnEnter(StateMachineContext context) {
            context.CooldownManager.Get(_timePerPointID).Resume();
        }

        public void OnExit(StateMachineContext context) {
            _routes[_contextToRoute[context]].CurrentCount--;
            _contextToRoute.Remove(context);
        }

        public void OnPatrolPointFinish(StateMachineContext context) {
            context.CooldownManager.Get(_timePerPointID).Start();
        }

        public void OnUpdate(StateMachineContext context, float dt) { }

        public int Prev(StateMachineContext context, int index) {
            return index - 1 >= 0 ? index - 1 : _routes[_contextToRoute[context]].Points.Length - 1;
        }

        public void TickPatrolPoint(StateMachineContext context, float dt) {
            context.CooldownManager.Get(_timePerPointID).Update(dt);
        }
    }
}
```
