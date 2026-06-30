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

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;

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
            DashPressed = _dashAction.WasPressedThisFrame();
        }
    }
}
