using UnityEngine;

namespace AI.Adapters {
    public class RigidBodyAdapter : MovementAdapter {
        [SerializeField] private float _turnSpeed = 360.0f;
        [SerializeField] private float _gravity = 30.0f;
        [SerializeField] private Rigidbody _rb;

        [SerializeField] private float _currentSpeed;
        [SerializeField] private float _targetSpeed;

        [SerializeField] private Vector3 _target;
        [SerializeField] private bool _hasTarget = false;

        [SerializeField] private bool _grounded;
        [SerializeField] private LayerMask _groundLayer;

        [SerializeField] private Vector3 _velocity;
        private RaycastHit _hit;
        private Vector3 _groundNormal;
        private Collider[] _groundChecks = new Collider[1];

        [SerializeField] private int _obstacleAvoidanceRays = 8;
        private float[] _directionWeights;

        ///<summary>Target position to navigate to</summary>
        public override Vector3 Target => _target;

        private void OnValidate() {
            _rb = GetComponent<Rigidbody>();

            _directionWeights = new float[_obstacleAvoidanceRays];

            if (_groundLayer == 0) {
                _groundLayer = int.MaxValue - (1 << gameObject.layer);
            }
        }

        ///<summary>Move using velocity</summary>
        ///<param name="velocity">Velocity to move with (not multiplied by either Time.fixedDeltaTime or Time.deltaTime)</param>
        public override void Move(Vector3 velocity) {
            _hasTarget = false;
            _velocity = velocity;
            _rb.linearVelocity = velocity;
        }

        ///<summary>Start moving toward destination (without pathfinding)</summary>
        ///<param name="targetPosition">Position to move to - used as delta to provide direction</param>
        public override void SetDestination(Vector3 targetPosition) {
            _hasTarget = true;
            _target = targetPosition;
        }

        private void FixedUpdate() {
            CheckGrounded();
            Vector3 movement = GetBaseMovement();

            if (movement.sqrMagnitude >= 1f) {
                AvoidObstacles(ref movement);
            }

            if (movement.WithY(0).sqrMagnitude >= 0.01f) {
                transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        Quaternion.LookRotation(movement.WithY(0).normalized, Vector3.up),
                        _turnSpeed * Time.fixedDeltaTime);
            }

            if (_grounded) {
                _rb.linearVelocity = Vector3.ProjectOnPlane(movement, _groundNormal);
            } else {
                _rb.linearVelocity = movement;
            }


            _velocity = movement;
        }

        private void AvoidObstacles(ref Vector3 velocity) {
            float angle = transform.eulerAngles.y;
            float increase = 360.0f / Mathf.Max(1.0f, _directionWeights.Length);

            RaycastHit hit;
            Vector3 velDir = velocity.normalized;
            Vector3 cumulative = Vector3.zero;

            for (int i = 0; i < _directionWeights.Length; i++) {
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;

                _directionWeights[i] = 0.5f + 0.5f * Vector3.Dot(velDir, dir);

                if (Physics.Raycast(transform.position, dir, out hit, 5.0f, _groundLayer) && hit.collider.gameObject.layer != gameObject.layer) {
                    _directionWeights[i] -= 1.0f - (hit.distance / 5.0f);
                }

                angle += increase;
                cumulative += dir * _directionWeights[i];
            }

            velocity = cumulative.normalized * velocity.WithY(0).magnitude;
        }

        private Vector3 GetBaseMovement() {
            if (_hasTarget) {
                float distance = Vector3.Distance(_target.WithY(0), _rb.position.WithY(0));

                if (distance <= 0.01f) {
                    _rb.linearVelocity = Vector3.zero;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation((_target - _rb.position).WithY(0).normalized, Vector3.up), _turnSpeed * Time.fixedDeltaTime);
                    return Vector3.zero;
                }

                _targetSpeed = Mathf.Min(distance * _maxSpeed, _maxSpeed);
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, Time.fixedDeltaTime * _acceleration);
                Vector3 velocity = Vector3.MoveTowards(Vector3.zero, (_target - _rb.position).WithY(0), _currentSpeed);
                velocity.y = -_gravity * Time.fixedDeltaTime;

                return velocity;
            } else {
                return _velocity;
            }
        }

        private void OnDrawGizmos() {

            if (!Application.isPlaying) { return; }

            Vector3 velocity = GetBaseMovement();

            float angle = 0.0f;
            float increase = 360.0f / (float) _obstacleAvoidanceRays - 1;

            RaycastHit hit;

            Vector3 cumulative = Vector3.zero;

            for (int i = 0; i < _directionWeights.Length; i++) {
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;

                _directionWeights[i] = 0.5f + 0.5f * Vector3.Dot(velocity.normalized, dir);

                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, dir * _directionWeights[i]);

                if (Physics.Raycast(transform.position, dir, out hit, 5.0f, _groundLayer) && hit.collider.gameObject.layer != gameObject.layer) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(transform.position, dir * (1.0f - (hit.distance / 5.0f)));
                    _directionWeights[i] -= 1.0f - (hit.distance / 5.0f);
                }


                angle += increase;
                if (_directionWeights[i] >= 0.0f) {
                    cumulative += dir * _directionWeights[i];
                }
            }

            Gizmos.color = Color.yellow;
            velocity = cumulative.normalized * velocity.WithY(0).magnitude;
            Gizmos.DrawRay(transform.position, velocity);
        }

        ///<summary>Current speed</summary>
        public override float Speed() {
            return _rb.linearVelocity.WithY(0).magnitude;
        }

        ///<summary>Current velocity</summary>
        public override Vector3 Velocity() {
            return _rb.linearVelocity;
        }

        private void CheckGrounded() {
            bool prev = _grounded;

            _grounded = Physics.OverlapSphereNonAlloc(transform.position, 0.05f, _groundChecks, _groundLayer) > 0;
            if (_grounded && Physics.Raycast(transform.position, Vector3.down, out _hit, 1.0f, _groundLayer)) {
                _groundNormal = _hit.normal;
            }
        }
    }
}