using System;
using System.Collections.Generic;

using UnityEngine;

namespace AI.Adapters {

    ///<summary>Collects colliders that collide with the object matching mask and condition</summary>
    public class ColliderCollector : MonoBehaviour {
        public readonly HashSet<Transform> Transforms = new HashSet<Transform>();
        public readonly HashSet<Collider> Colliders = new HashSet<Collider>();

        [Header("Collection Settings")]
        [SerializeField] private bool _allowCollection = false;
        [SerializeField] private int _defaultCapacity = 32;
        [SerializeField] private LayerMask _mask = -1;

        [Header("Debug")]
        [SerializeField] private bool _debug;
        [SerializeField] private List<Collider> _colliders;

        private Func<GameObject, bool> _targetFilter = delegate { return true; };
        private Action<GameObject> _onHitCallback = delegate { };

        private void Awake() {
            Transforms.EnsureCapacity(_defaultCapacity);
            Colliders.EnsureCapacity(_defaultCapacity);
            _colliders = new List<Collider>(_defaultCapacity);
            _targetFilter = (GameObject _) => true;
        }

        ///<summary>Sets filter function for colliders - can be null</summary>
        ///<param name="filter">Function used in OnTriggerEnter to filter colliders</param>
        public void SetTargetFilter(Func<GameObject, bool> filter) {
            _targetFilter = filter;
        }

        ///<summary>Sets callback upon collecting new object</summary>
        ///<param name="callback">Callback to invoke - null will disable callback</param>
        public void SetOnHitCallback(Action<GameObject> callback) {
            if (callback == null) {
                _onHitCallback = delegate { };
            } else {
                _onHitCallback = callback;
            }
        }

        ///<summary>Enables collection</summary>
        ///<param name="clearCollections">Optionally clear cached collections</param>
        public void EnableCollection(bool clearCollections = true) {
            if (clearCollections) {
                ClearCollections();
            }
            _allowCollection = true;
        }

        ///<summary>Disables collection</summary>
        ///<param name="clearCollections">Optionally clear cached collections</param>
        public void DisableCollection(bool clearCollections = false) {
            _allowCollection = false;

            if (clearCollections) {
                ClearCollections();
            }
        }

        ///<summary>Toggles collection state</summary>
        ///<param name="clearCollections">Optionally clear cached collections</param>
        ///<returns>New collection state (true if collection is enabled)</returns>
        public bool ToggleCollection(bool clearCollections) {
            _allowCollection = enabled;

            if (clearCollections) {
                ClearCollections();
            }

            return _allowCollection;
        }

        ///<summary>Sets collection state</summary>
        ///<param name="enabled">New state (true to enable collection)</param>
        public void SetCollection(bool enabled) {
            _allowCollection = enabled;
        }

        ///<summary>Clears all cached collections</summary>
        public void ClearCollections() {
            Transforms.Clear();
            Colliders.Clear();
            if (_debug) {
                _colliders.Clear();
            }
        }

        private bool IsValidObject(GameObject obj) {
            if (_targetFilter == null) {
                return true;
            } else {
                return _targetFilter.Invoke(obj);
            }
        }

        private void OnTriggerEnter(Collider collider) {
            if (_allowCollection && (_mask.value & 1 << collider.gameObject.layer) > 0 && IsValidObject(collider.gameObject) && !Transforms.Contains(collider.transform)) {
                Transforms.Add(collider.transform);
                Colliders.Add(collider);
                _onHitCallback.Invoke(collider.gameObject);
                if (_debug) {
                    _colliders.Add(collider);
                }
            }
        }
    }
}