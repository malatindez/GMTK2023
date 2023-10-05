using UnityEngine;

namespace EasyCharacterMovement.Examples.InputExamples.CharacterControllerExample
{
    /// <summary>
    /// This example shows how to control a character from an external script.
    /// </summary>

    public class MyCharacterController : MonoBehaviour
    {
        public Camera followCamera;
        public Character character;

        private void Awake()
        {
            // Disable character input (ie: its HandleInput method)
            
            character.handleInput = false;
        }

        private void Update()
        {
            // Read movement input

            Vector2 movementInput = new Vector2
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };

            // Add movement input relative to camera's view direction (in world space)

            Vector3 movementDirection = Vector3.zero;

            movementDirection += Vector3.right * movementInput.x;
            movementDirection += Vector3.forward * movementInput.y;

            if (followCamera != null)
            {
                movementDirection = movementDirection.relativeTo(followCamera.transform);
            }

            character.SetMovementDirection(movementDirection);

            // Jump

            if (Input.GetButton("Jump"))
            {
                character.Jump();
            }
            else if (Input.GetButtonUp("Jump"))
            {
                character.StopJumping();
            }

            // Crouch 

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
            {
                character.Crouch();
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
            {
                character.StopCrouching();
            }

            // Sprint

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                character.Sprint();
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                character.StopSprinting();
            }            
        }
    }
}
