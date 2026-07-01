using UnityEngine;
using UnityEngine.InputSystem;

namespace Hollow.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpReleased { get; private set; }
        public bool DashPressed { get; private set; }
        public bool DashHeld { get; private set; }
        public bool DashReleased { get; private set; }
        public bool DashTap { get; private set; }
        public bool DashHoldExceeded { get; private set; }
        public bool WantsDownDash { get; private set; }

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;

        private float _dashHoldTime;
        private bool _dashTracking;
        private float _dashTapThreshold = 0.15f;

        public void SetDashTapThreshold(float threshold)
        {
            _dashTapThreshold = threshold;
        }

        private void Awake()
        {
            var playerInput = GetComponent<PlayerInput>();
            var actions = playerInput.actions;

            _moveAction = actions["Move"];
            _jumpAction = actions["Jump"];
            _dashAction = actions["Sprint"];
        }

        private void Update()
        {
            MoveInput = _moveAction.ReadValue<Vector2>();
            JumpPressed = _jumpAction.WasPressedThisFrame();
            JumpHeld = _jumpAction.IsPressed();
            JumpReleased = _jumpAction.WasReleasedThisFrame();

            var dashCurrentlyHeld = _dashAction.IsPressed();
            DashHeld = dashCurrentlyHeld;
            DashReleased = _dashAction.WasReleasedThisFrame();
            DashPressed = _dashAction.WasPressedThisFrame();
            DashTap = false;
            DashHoldExceeded = false;

            if (DashPressed)
            {
                _dashHoldTime = 0f;
                _dashTracking = true;
            }

            if (_dashTracking)
            {
                if (dashCurrentlyHeld)
                {
                    _dashHoldTime += Time.deltaTime;
                    if (_dashHoldTime >= _dashTapThreshold)
                        DashHoldExceeded = true;
                }

                if (DashReleased)
                {
                    DashTap = _dashHoldTime < _dashTapThreshold;
                    _dashTracking = false;
                }
            }

            WantsDownDash = MoveInput.y < -0.5f;
        }
    }
}
