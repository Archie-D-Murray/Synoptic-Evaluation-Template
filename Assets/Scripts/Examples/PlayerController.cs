using System;

using Utilities;

using UnityEngine;
using AI.Adapters;

namespace AI.Examples {

    ///<summary>Basic Third Person Movement - Most likely should be replaced with your own</summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AnimationAdapter))]
    public class PlayerController : MonoBehaviour {
        [Serializable]
        class SpeedParams {
            public float TargetSpeed = 5.0f;
            public float Acceleration = 8.0f;
            public float Deceleration = 5.0f;

            public SpeedParams(float targetSpeed, float acceleration, float deceleration) {
                TargetSpeed = targetSpeed;
                Acceleration = acceleration;
                Deceleration = deceleration;
            }
        }

        [Header("Movement")]
        [SerializeField] private SpeedParams _groundParams = new SpeedParams(5.0f, 14.0f, 10.0f);
        [SerializeField] private SpeedParams _airParams = new SpeedParams(5.0f, 2.0f, 2.0f);
        [SerializeField] private SpeedParams _airStrafeParams = new SpeedParams(1.0f, 30.0f, 30.0f);
        [SerializeField] private float _airModifier = 0.5f;
        [SerializeField] private float _airStrafeMagicConstant = 20.0f;
        [SerializeField] private float _friction = 5.0f;
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _gravity = 30.0f;
        [SerializeField] private CountDownTimer _jumpTimer = new CountDownTimer(0.25f);
        [SerializeField] private bool _grounded = true;
        [SerializeField] private LayerMask _ground;
        [SerializeField] private Vector3 _groundNormal = Vector3.down;
        [SerializeField] private float _turnSpeed = 360.0f;
        [SerializeField] private float _sprintModifier = 1.5f;

        [Space, Header("Camera")]
        [SerializeField] private Vector2 _sensitivity = new Vector2(1.0f, -1.0f);
        [SerializeField] private float _mouseSpeed = 50.0f;
        [SerializeField] private float _camY = 0.0f;
        [SerializeField] private float _camX = 0.0f;
        [SerializeField] private float _camMin = -85.0f;
        [SerializeField] private float _camMax = 85.0f;
        [SerializeField] private bool _canMove;
        [SerializeField] private Vector3 _localCameraOffset = Vector3.up * 1.5f;

        [Space, Header("Debug")]
        [SerializeField] private Vector3 _input;
        [SerializeField] private Vector3 _targetDir;

        private Rigidbody _rb;
        private Camera _cam;
        private RaycastHit _hit;
        private AnimationAdapter _animator;
        private PlayerInputs _inputs;

        private void OnValidate() {
            _inputs = GetComponent<PlayerInputs>();
            _cam = Camera.main;
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<AnimationAdapter>();
        }

        private void Start() {
            _inputs.OnLockStateChange += SetCanMove;
            GetComponent<Health>().OnDamage += HUD.Instance.OnDamage;
        }

        private void Update() {
            if (_inputs.Jump && _grounded && _jumpTimer.IsFinished) {
                Jump();
            }
        }

        private void FixedUpdate() {

            CheckGrounded();

            _input = Vector3.ClampMagnitude(_inputs.Move, 1.0f);

            _jumpTimer.Update(Time.fixedDeltaTime);

            _targetDir = RotateMovement(_input);

            if (_canMove) {
                if (_grounded && _jumpTimer.IsFinished) {
                    GroundedMovement(_input);
                } else {
                    AirMovement(_input);
                }
                if (_input.sqrMagnitude >= 0.01f) {
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.LookRotation(RotateMovement(_input)), _turnSpeed * Time.deltaTime);
                }
                _rb.angularVelocity = Vector3.zero;
            } else {
                _input = Vector3.zero;
                if (_grounded && _hit.distance <= 0.01f) {
                    _rb.linearVelocity = Vector3.zero;
                } else {
                    _rb.linearVelocity = Vector3.down * _gravity * Time.fixedDeltaTime;
                }
                _rb.angularVelocity = Vector3.zero;
            }

            _animator.SetFloat(AIAnimationParam.Speed, _rb.linearVelocity.magnitude / _groundParams.TargetSpeed);
        }

        private void LateUpdate() {
            Look();
        }

        private void SetCanMove(bool cursorLocked) {
            _canMove = cursorLocked;
        }

        private void Jump() {
            _jumpTimer.Start();
            float vertical = Mathf.Sqrt(_jumpHeight * 2f * _gravity);
            _rb.linearVelocity = _rb.linearVelocity.WithY(vertical);
        }

        private void GroundedMovement(Vector3 input) {
            Vector3 velocity = _rb.linearVelocity;

            // Apply friction if no jump pressed
            if (!_inputs.Jump) {
                float speed = _rb.linearVelocity.WithY(0).magnitude;

                float friction = Mathf.Min(speed, _groundParams.Deceleration) * _friction * Time.fixedDeltaTime;

                float reducedSpeed = Mathf.Max(0, speed - friction);
                if (speed > 0) {
                    reducedSpeed /= speed;
                }

                velocity.x *= reducedSpeed;
                velocity.z *= reducedSpeed;
            }

            Vector3 movement = RotateMovement(input);

            velocity = Accelerate(velocity, movement, _groundParams.TargetSpeed * GetSpeedModifier(), _groundParams.Acceleration * Time.fixedDeltaTime);
            velocity.y = -_gravity * Time.fixedDeltaTime;

            _rb.linearVelocity = Vector3.ProjectOnPlane(velocity, _groundNormal);
        }

        private float GetSpeedModifier() {
            return _inputs.Sprint ? _sprintModifier : 1.0f;
        }

        private void AirMovement(Vector3 input) {
            Vector3 velocity = _rb.linearVelocity;

            Vector3 movement = RotateMovement(input);
            float targetSpeed = movement.magnitude * _airParams.TargetSpeed;
            float acceleration = Vector3.Dot(velocity, movement) >= 0 ? _airParams.Acceleration : _airParams.Deceleration;

            // Use strafe params instead as no forward/backward movement
            if (input.z == 0 && input.x != 0) {
                targetSpeed = Mathf.Min(targetSpeed, _airStrafeParams.TargetSpeed);
                acceleration = _airStrafeParams.Acceleration;
            }

            velocity = Accelerate(velocity, movement, targetSpeed, acceleration * Time.fixedDeltaTime);

            if (_airModifier > 0 && targetSpeed > 0.01f) {
                float yVelocity = velocity.y;
                velocity.y = 0;

                float speed = velocity.magnitude;
                if (speed > 0.0001f) {
                    Vector3 targetDir = movement.normalized;
                    Vector3 velDir = velocity.normalized;

                    float dot = Vector3.Dot(velDir, targetDir);

                    if (dot > 0) {
                        float control = _airModifier * dot * dot * Time.fixedDeltaTime * _airStrafeMagicConstant;

                        velocity = velDir * speed + targetDir * control;
                        velocity = velocity.normalized * speed;
                    }
                }

                velocity.y = yVelocity;
            }

            velocity.y -= _gravity * Time.fixedDeltaTime;

            _rb.linearVelocity = velocity;
        }

        private void Look() {
            Vector2 mouseInput = _inputs.Look * _sensitivity * _mouseSpeed * Time.deltaTime;
            if (!_canMove) { mouseInput = Vector2.zero; }
            _camX = Helpers.ClampAngle(_camX + mouseInput.y, _camMin, _camMax);
            _camY += Helpers.ClampAngle(mouseInput.x, -180.0f, 180.0f);
            _cam.transform.rotation = Quaternion.Euler(_camX, _camY, 0);
            _cam.transform.position = transform.position.Offset(y: 2.0f) + Quaternion.Euler(_camX, _camY, 0) * _localCameraOffset;
        }

        Vector3 RotateMovement(Vector3 input) {
            if (input.sqrMagnitude <= 0.01f) {
                return Vector3.zero;
            }
            return Quaternion.AngleAxis(_camY, Vector3.up) * input;
        }

        private Vector3 Accelerate(Vector3 velocity, Vector3 direction, float targetSpeed, float acceleration) {
            float currentSpeed = Vector3.Dot(velocity, direction);

            if (currentSpeed >= targetSpeed) { return velocity; }

            float delta = targetSpeed - currentSpeed;
            acceleration = Mathf.Min(delta, acceleration * targetSpeed);

            velocity.x += acceleration * direction.x;
            velocity.z += acceleration * direction.z;
            return velocity;
        }

        private void CheckGrounded() {
            bool old = _grounded;
            _grounded = Physics.OverlapSphere(transform.position, 0.05f, _ground).Length > 0;
            if (_grounded && Physics.Raycast(transform.position, Vector3.down, out _hit, 1.0f, _ground)) {
                _groundNormal = _hit.normal;
            }
        }
    }
}