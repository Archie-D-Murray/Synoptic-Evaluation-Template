using System;

using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AI.Examples {
    ///<summary>Basic Input handling allowing either input system</summary>
    ///<summary>Designed for both systems at their default state - should be refactored/replaced</summary>
    public class PlayerInputs : MonoBehaviour {
        public Vector3 Move;
        public Vector2 Look;
        public bool Jump;
        public bool Sprint;
        public bool LockCursor = true;

        public Action<bool> OnLockStateChange = delegate { };

#if ENABLE_INPUT_SYSTEM
        Inputs _input;

        private void Start() {
            if (_input != null) {
                _input.Disable();
                _input.Dispose();
            }
            _input = new Inputs();

            _input.Player.Move.started += OnMove;
            _input.Player.Move.performed += OnMove;
            _input.Player.Move.canceled += OnMove;

            _input.Player.Look.started += OnLook;
            _input.Player.Look.performed += OnLook;
            _input.Player.Look.canceled += OnLook;

            _input.Player.Sprint.started += OnSprint;
            _input.Player.Sprint.performed += OnSprint;
            _input.Player.Sprint.canceled += OnSprint;

            _input.Player.Jump.started += OnJump;
            _input.Player.Jump.performed += OnJump;
            _input.Player.Jump.canceled += OnJump;

            _input.Player.UnlockMouse.performed += OnCancel;
            _input.Player.UnlockMouse.started += OnCancel;
            _input.Player.UnlockMouse.canceled += OnCancel;

            _input.Enable();
            if (LockCursor) {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnEnable() {
            if (_input != null) {
                _input.Enable();
            }
        }

        private void OnDisable() {
            _input.Disable();
        }

        private void OnDispose() {
            _input.Dispose();
        }

        public void OnMove(InputAction.CallbackContext ctx) {
            SetMove(ctx.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext ctx) {
            SetLook(ctx.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext ctx) {
            SetJump(ctx.ReadValueAsButton());
        }

        public void OnSprint(InputAction.CallbackContext ctx) {
            SetSprint(ctx.ReadValueAsButton());
        }

        public void OnCancel(InputAction.CallbackContext ctx) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            LockCursor = Cursor.lockState == CursorLockMode.Locked;
            OnLockStateChange.Invoke(LockCursor);
        }

#else
        [SerializeField] private KeyCode _freeMouse = KeyCode.F9;
#endif

        private void Update() {
#if ENABLE_INPUT_SYSTEM
#else
            if (Input.GetKeyDown(_freeMouse) || Input.GetKeyDown(KeyCode.Escape)) {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
                LockCursor = Cursor.lockState == CursorLockMode.Locked;
                OnLockStateChange.Invoke(LockCursor);
            }

            SetMove(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
            SetLook(Input.mousePositionDelta);
            SetJump(Input.GetButton("Jump"));
            SetSprint(Input.GetButton("Fire3"));
#endif
        }

        public void SetMove(Vector2 move) {
            Move = move.ToXZ();
        }

        private void SetLook(Vector2 look) {
            Look = look;
        }

        private void SetJump(bool jump) {
            Jump = jump;
        }

        private void SetSprint(bool sprint) {
            Sprint = sprint;
        }
    }
}