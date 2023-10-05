using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyCharacterMovement
{
    /// <summary>
    /// ThirdPersonCharacter.
    ///
    /// This extends the Character class to add controls for a typical third person movement.
    /// </summary>

    public class ThirdPersonCharacter : Character
    {
        #region FIELDS

        private ThirdPersonCameraController _cameraController;

        #endregion

        #region PROPERTIES


        /// <summary>
        /// Cached camera controller.
        /// </summary>
        
        protected ThirdPersonCameraController cameraController
        {
            get
            {
                if (_cameraController == null)
                    _cameraController = camera.GetComponent<ThirdPersonCameraController>();

                return _cameraController;
            }
        }

        #endregion

        #region METHODS        

        /// <summary>
        /// Perform camera related input actions, eg: Look Up / Down, Turn, etc.
        /// </summary>

        protected virtual void HandleCameraInput()
        {
            // If Character is disabled, halts camera input

            if (IsDisabled())
                return;

            // Cursor lock / unlock

            if (Input.GetMouseButtonUp(0))
            {
                cameraController.LockCursor();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                cameraController.UnlockCursor();
            }

            if (!cameraController.IsCursorLocked())
            {
                // Disable mouse look when cursor is unlocked

                return;
            }

            // Mouse look
            
            Vector2 mouseLookInput = new Vector2
            {
                x = Input.GetAxisRaw("Mouse X"),
                y = Input.GetAxisRaw("Mouse Y"),
            };

            if (mouseLookInput.sqrMagnitude > 0)
            {
                // Mouse look input

                if (mouseLookInput.x != 0.0f)
                    cameraController.Turn(mouseLookInput.x);

                if (mouseLookInput.y != 0.0f)
                    cameraController.LookUp(mouseLookInput.y);

            }

            // Mouse scroll input

            float mouseScrollInput = Input.GetAxisRaw("Mouse ScrollWheel");

            if (mouseScrollInput != 0.0f)
                cameraController.ZoomAtRate(mouseScrollInput);

            // Mouse lock / unlock

            if (Input.GetMouseButtonDown(0))
            {
                _cameraController.LockCursor();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                _cameraController.UnlockCursor();
            }
        }

        /// <summary>
        /// Extends the HandleInput method to add Camera related inputs.
        /// </summary>

        protected override void HandleInput()
        {
            base.HandleInput();

            HandleCameraInput();
        }

        #endregion
    }
}