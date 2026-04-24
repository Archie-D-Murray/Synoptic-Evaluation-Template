using UnityEngine;

namespace AI.Adapters {
    ///<summary>Used as a base to determine a target for things like chasing and attacking</summary>
    public abstract class DetectorAdapter : MonoBehaviour {

        [Header("Target Data")]
        [SerializeField] protected Transform _target = null;
        [SerializeField] protected Vector3 _lastTargetPosition;
        [SerializeField] protected bool _justLostTarget;
        ///<summary>Updates the current target if current is either null or not considered 'visible'</summary>
        ///<returns>New target - may be null</returns>
        protected abstract Transform FindTarget();

        ///<summary>Current target - may be null</summary>
        public Transform Target => _target;

        ///<summary>Current target position - will used last known position if now null</summary>
        public Vector3 TargetPosition => _target ? _target.position : _lastTargetPosition;

        ///<summary>Was the target lost between now and last FindTarget call</summary>
        public bool JustLostTarget => _justLostTarget;

        ///<summary>Do we have a non null target</summary>
        public bool HasTarget() {
            return Target;
        }
    }
}