using System.Collections.Generic;

using UnityEngine;

using Utilities;

namespace AI.Adapters {
    ///<summary>Determines nearest target within a range or previous if still in range</summary>
    public class RangeDetectorAdapter : DetectorAdapter {

        [Space, Header("Adaptor")]
        [SerializeField] private Vector3 _offset = Vector3.up * 1.5f;
        [SerializeField] private float _range = 5.0f;

        ///<summary>Layer Mask for Physics.OverlapSphere()</summary>
        [SerializeField] private LayerMask _mask = -1;

        ///<summary>Duration between FindTarget calls</summary>
        [SerializeField] private CountDownTimer _searchTimer = new CountDownTimer(0.1f);

        protected Collider[] _found = new Collider[32];
        protected HashSet<Transform> _unique = new HashSet<Transform>(32);

        ///<summary>Updates current target</summary>
        ///<returns>Nearest target if no target or old if still in range, null if nothing was found</returns>
        protected override Transform FindTarget() {
            _justLostTarget = false;
            Transform prev = _target;

            if (_target && Vector3.SqrMagnitude(_target.position - transform.position) <= _range * _range) {
                _lastTargetPosition = _target.position;
                return _target;
            } else if (_target) {
                _target = null;
                _justLostTarget = true;
            }

            Transform closest = null;
            if (!_target) {
                int count = Physics.OverlapSphereNonAlloc(transform.position + _offset, _range, _found, _mask);

                _unique.Clear();
                float dist = float.MaxValue;
                for (int i = 0; i < count; i++) {
                    if (_found[i].transform.ContainsParentInHierarchy(transform.root)) { continue; }
                    if (_unique.Add(_found[i].transform)) {
                        float distance = (_found[i].transform.position - transform.position).sqrMagnitude;
                        if (dist > distance) {
                            dist = distance;
                            closest = _found[i].transform;
                        }
                    }
                }
            }

            if (_unique.Contains(_target)) {
                _lastTargetPosition = _target.position;
                return _target;
            } else if (closest) {
                _lastTargetPosition = closest.position;
                return closest;
            } else {
                _target = null;
                if (prev && !_target) {
                    _justLostTarget = true;
                }
                return null;
            }
        }

        protected void Awake() {
            _searchTimer.Start();
        }

        ///<summary>Searches for a new target every time search timer finishes</summary>
        protected void FixedUpdate() {
            _searchTimer.Update(Time.fixedDeltaTime);
            if (_searchTimer.IsFinished) {
                _target = FindTarget();
                _searchTimer.Reset();
                _searchTimer.Start();
            }
        }
    }
}