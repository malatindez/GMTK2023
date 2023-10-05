using UnityEngine;

namespace EasyCharacterMovement.Examples.Gameplay.PlanetWalkExample
{
    /// <summary>
    /// This example extends the third person character class so it can walk around a 3d planet like Mario Galaxy.
    /// </summary>
    
    public sealed class MyCharacter : ThirdPersonCharacter
    {
        [Space(15f)]
        public Transform planet;

        /// <summary>
        /// Handles the character input.
        /// Unlike default input, this makes use of a secondary relativeTo extension method which allows
        /// to set an arbitrary up axis, in this case our character's current up direction.
        /// </summary>
        
        private void HandleCharacterInput()
        {
            // Movement

            Vector2 movementInput = new Vector2 { x = Input.GetAxisRaw("Horizontal"), y = Input.GetAxisRaw("Vertical") };

            Vector3 movementDirection = Vector3.zero;
            
            movementDirection += Vector3.right * movementInput.x;
            movementDirection += Vector3.forward * movementInput.y;
            
            movementDirection = movementDirection.relativeTo(cameraTransform, GetUpVector());
            
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
        }

        /// <summary>
        /// Extends the HandleInput method to add Camera related inputs.
        /// </summary>
        
        protected override void HandleInput()
        {
            HandleCameraInput();

            HandleCharacterInput();            
        }

        /// <summary>
        /// Updates the Character's rotation based on its current RotationMode PLUS its current up direction.
        /// </summary>
        
        protected override void UpdateRotation()
        {
            // Call base method (i.e: rotate towards movement direction)

            base.UpdateRotation();

            // Updates gravity direction and orient Character's Up to -gravity direction
            
            gravity = (transform.position - planet.position).normalized * -gravity.magnitude;

            characterMovement.rotation = Quaternion.FromToRotation(GetUpVector(), -gravity) * GetRotation();
        }


    }
}