using UnityEngine;

namespace EasyCharacterMovement.Examples.Gameplay.TargetLockExample
{
    /// <summary>
    /// This example shows how to extend a Character to perform a target locking mechanic.
    /// </summary>

    public class TargetLockCharacter : Character
    {
        #region EDITOR EXPOSED FIELDS

        [Header("Target")]
        public Transform targetTransform;

        #endregion

        #region FIELDS

        private bool _lockButtonPressed;

        #endregion

        #region METHODS

        /// <summary>
        /// Start a target lock.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public void LockTarget()
        {
            _lockButtonPressed = true;
        }

        /// <summary>
        /// Stops target locking.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public void StopLockingTarget()
        {
            _lockButtonPressed = false;
        }

        /// <summary>
        /// Is the Character looking at its target ?
        /// </summary>

        public bool IsLockingTarget()
        {
            return targetTransform != null && _lockButtonPressed;
        }

        /// <summary>
        /// Extends Handle Input method to handle target locking state.
        /// </summary>

        protected override void HandleInput()
        {
            if (!IsLockingTarget())
            {
                // If not locking, perform regular movement (i.e: Orient to movement direction

                base.HandleInput();
            }
            else
            {
                // Add movement relative to us (looking at target)

                Vector2 movementInput = new Vector2
                {
                    x = Input.GetAxisRaw("Horizontal"),
                    y = Input.GetAxisRaw("Vertical")
                };

                Vector3 movementDirection = Vector3.zero;

                movementDirection += GetRightVector() * movementInput.x;
                movementDirection += GetForwardVector() * movementInput.y;

                SetMovementDirection(movementDirection);
            }

            // Target lock / unlock input

            if (Input.GetKeyDown(KeyCode.E))
            {
                LockTarget();
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                StopLockingTarget();
            }
        }

        /// <summary>
        /// Extends UpdateRotation method to handle target locking state.
        /// </summary>

        protected override void UpdateRotation()
        {
            // If not locking target use default rotation

            if (!IsLockingTarget())
            {
                base.UpdateRotation();
            }
            else
            {
                // Look at target

                Vector3 toTarget = targetTransform.position - GetPosition();

                RotateTowards(toTarget);
            }
        }

        #endregion
    }
}
