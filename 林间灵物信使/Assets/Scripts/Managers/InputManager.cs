using UnityEngine;
using ForestMessenger.Core;

namespace ForestMessenger.Managers
{
    public class InputManager : Singleton<InputManager>
    {
        public delegate void JumpPressed();
        public event JumpPressed OnJumpPressed;

        public delegate void JumpReleased();
        public event JumpReleased OnJumpReleased;

        public float HorizontalInput { get; private set; }
        public float VerticalInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool JumpHeld { get; private set; }

        private void Update()
        {
            HandleMovementInput();
            HandleJumpInput();
        }

        private void HandleMovementInput()
        {
            HorizontalInput = Input.GetAxisRaw("Horizontal");
            VerticalInput = Input.GetAxisRaw("Vertical");
        }

        private void HandleJumpInput()
        {
            if (Input.GetButtonDown("Jump"))
            {
                JumpInput = true;
                JumpHeld = true;
                OnJumpPressed?.Invoke();
            }

            if (Input.GetButtonUp("Jump"))
            {
                JumpInput = false;
                JumpHeld = false;
                OnJumpReleased?.Invoke();
            }
        }

        public Vector2 GetMovementInput()
        {
            return new Vector2(HorizontalInput, VerticalInput);
        }
    }
}
