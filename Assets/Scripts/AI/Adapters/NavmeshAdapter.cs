using UnityEngine;

using UnityEngine.AI;

namespace AI.Adapters {
    public class NavmeshAdapter : MovementAdapter {
        private NavMeshAgent _agent;
        private NavMeshHit _hit;
        private float _maxDistance = 1.0f;

        ///<summary>Target position to navigate to</summary>
        public override Vector3 Target => _agent.destination;

        ///<summary>Is a path being computed</summary>
        public override bool PathPending => _agent.pathPending;

        ///<summary>Manually drive agent velocity</summary>
        ///<param name="velocity">Velocity to move with (not multiplied by either Time.fixedDeltaTime or Time.deltaTime)</param>
        public override void Move(Vector3 velocity) {
            _agent.velocity = velocity;
        }

        ///<summary>Start navigating to a destination</summary>
        ///<param name="targetPosition">Position to move to - will be sampled if using navmesh</param>
        public override void SetDestination(Vector3 targetPosition) {
            if (NavMesh.SamplePosition(targetPosition, out _hit, _maxDistance, NavMesh.AllAreas)) {
                _agent.SetDestination(_hit.position);
            }
        }

        ///<summary>Current speed</summary>
        public override float Speed() {
            return _agent.velocity.magnitude;
        }

        ///<summary>Current velocity</summary>
        public override Vector3 Velocity() {
            return _agent.velocity;
        }
    }
}