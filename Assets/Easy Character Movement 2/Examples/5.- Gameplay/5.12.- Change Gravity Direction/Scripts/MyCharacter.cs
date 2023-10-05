using UnityEngine;

namespace EasyCharacterMovement.Examples.Gameplay.ChangeGravityDirectionExample
{
    /// <summary>
    /// This example shows how to extend a Character to change Gravity direction at run-time.
    /// </summary>

    public class MyCharacter : Character
    {
        #region METHODS

        /// <summary>
        /// Overrides HandleInput method to replace the default input method with an horizontal only movement.
        /// </summary>

        protected override void HandleInput()
        {
            // Add horizontal only movement (in world space)

            Vector2 movementInput = new Vector2
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };

            Vector3 movementDirection = Vector3.right * movementInput.x;
            SetMovementDirection(movementDirection);

            // Jump

            if (Input.GetButton("Jump"))
            {
                Jump();
            }
            else if (Input.GetButtonUp("Jump"))
            {
                StopJumping();
            }

            // Toggle gravity direction if character is on air (e.g. Jumping)

            if (Input.GetKeyDown(KeyCode.E) && !IsGrounded())
            {
                gravityScale *= -1.0f;
            }
        }

        /// <summary>
        /// Extends UpdateRotation method to orient the Character towards gravity direction.
        /// </summary>

        protected override void UpdateRotation()
        {
            // Call base method implementation

            base.UpdateRotation();

            // Append gravity-direction rotation

            Quaternion targetRotation = Quaternion.FromToRotation(GetUpVector(), -GetGravityDirection()) * characterMovement.rotation;

            characterMovement.rotation = Quaternion.RotateTowards(characterMovement.rotation, targetRotation, rotationRate * deltaTime);
        }

        #endregion
    }
}
