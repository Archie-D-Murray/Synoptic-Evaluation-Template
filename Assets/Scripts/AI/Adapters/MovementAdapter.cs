using UnityEngine;

namespace AI.Adapters {

    ///<summary>Base movement class allowing for navigating to a destination and controlling velocity</summary>
    public abstract class MovementAdapter : MonoBehaviour {
        [SerializeField] protected float _maxSpeed = 1.0f;
        [SerializeField] protected float _acceleration = 1.0f;

        ///<summary>Target position to navigate to</summary>
        public abstract Vector3 Target { get; }

        ///<summary>Max Speed</summary>
        public float MaxSpeed => _maxSpeed;

        ///<summary>Normalized Speed (not in range [[0, 1]])</summary>
        public float NormalizedSpeed => _maxSpeed == 0.0f ? 0.0f : Speed() / _maxSpeed;

        ///<summary>Is a path being computed</summary>
        public virtual bool PathPending => false;

        ///<summary>Start navigating to a destination</summary>
        ///<param name="targetPosition">Position to move to - will be sampled if using navmesh</param>
        public abstract void SetDestination(Vector3 targetPosition);

        ///<summary>Move using velocity</summary>
        ///<param name="velocity">Velocity to move with (not multiplied by either Time.fixedDeltaTime or Time.deltaTime)</param>
        public abstract void Move(Vector3 velocity);

        ///<summary>Current speed</summary>
        public abstract float Speed();

        ///<summary>Current velocity</summary>
        public abstract Vector3 Velocity();
    }
}