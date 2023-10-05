using UnityEngine;

namespace EasyCharacterMovement.Examples.Gameplay.FirstPersonFlyingExample
{
    /// <summary>
    /// This example shows how to extend the FirstPersonCharacter to add flying movement.
    ///
    /// In this case, we allow to fly towards our look direction, allowing to freely move through the air.
    /// To implement the flying state, we use the Flying movement mode. The Flying movement mode needs to be manually enabled / disabled as needed.
    /// </summary>

    public class MyFirstPersonCharacter : FirstPersonCharacter
    {
        #region METHODS

        /// <summary>
        /// Extends OnCollided method to exit flying state when walkable ground is found.
        /// </summary>

        protected override void OnCollided(ref CollisionResult collisionResult)
        {
            // Call base method implementation

            base.OnCollided(ref collisionResult);

            // If flying and touched walkable ground, exit flying state.
            // I.e: change to falling movement mode as this is managed based on grounding status

            if (!IsGrounded() && collisionResult.isWalkable)
                SetMovementMode(MovementMode.Walking);
        }

        /// <summary>
        /// Determines if the Character is able to enter Flying state
        /// </summary>

        private bool CanFly()
        {
            // Cant fly if is on ground, or is jumping or not has jumped (eg: jumpCount == 0)

            return !IsOnGround() && !IsJumping() && jumpCount > 0;
        }
        
        /// <summary>
        /// Extends HandleInput method to handle flying.
        /// </summary>

        protected override void HandleInput()
        {
            // Handle default movement mode input

            base.HandleInput();

            if (!IsFlying())
            {
                // If wants to fly and CAN fly, enter flying state.
                // I.e: Launch character up and change to flying movement mode.

                if (jumpButtonPressed && CanFly())
                {
                    SetMovementMode(MovementMode.Flying);
                    
                    LaunchCharacter(-gravity * 0.5f);
                }
            }
            else
            {
                // If Flying, move towards eye view direction, and allow to strafe along out right vector and move up pressing jump button

                Vector2 movementInput = new Vector2
                {
                    x = Input.GetAxisRaw("Horizontal"),
                    y = Input.GetAxisRaw("Vertical")
                };

                Vector3 movementDirection = Vector3.zero;

                movementDirection += GetEyeForwardVector() * movementInput.y;
                movementDirection += GetRightVector() * movementInput.x;

                if (jumpButtonPressed)
                    movementDirection += GetUpVector();

                SetMovementDirection(movementDirection);
            }
        }

        #endregion
    }
}
