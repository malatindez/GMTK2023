using UnityEngine;

namespace EasyCharacterMovement.Examples.Gameplay.DashExample
{
    /// <summary>
    /// This example shows how to extend a Character to perform a dash.
    /// </summary>

    public class MyCharacter : Character
    {
        #region EDITOR EXPOSED FIELDS

        public float dashDuration = 0.1f;
        public float dashImpulse = 20.0f;

        #endregion

        #region FIELDS

        private bool _isDashing;
        private float _dashingTime;
        private Vector3 _dashDirection;

        #endregion

        #region METHODS
        
        /// <summary>
        /// Extends OnCollided method to stop dash on collision (i.e: against a wall).
        /// </summary>

        protected override void OnCollided(ref CollisionResult collisionResult)
        {
            // Call base method implementation

            base.OnCollided(ref collisionResult);

            // Ends dashing

            if (IsDashing() && !collisionResult.isWalkable)
                StopDashing();
        }

        /// <summary>
        /// Determines whether the character is dashing.
        /// </summary>
        
        public bool IsDashing()
        {
            return _isDashing;
        }

        /// <summary>
        /// Start a dash.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public void Dash()
        {
            // If already dashing, return

            if (IsDashing())
                return;
            
            // Enter dashing state,
            // Disable gravity, enable separate braking friction and apply dash impulse resenting character's velocity

            _isDashing = true;
            _dashingTime = 0.0f;

            useSeparateBrakingFriction = true;
            brakingFriction = 0.0f;

            EnableGravity(false);

            // Dash along movement direction (if moving) or along character's facing direction (if not moving)

            Vector3 movementDirection = GetMovementDirection();
            _dashDirection = movementDirection.isZero() ? GetForwardVector() : movementDirection;

            LaunchCharacter(_dashDirection * dashImpulse, true, true);
        }

        /// <summary>
        /// Stop the Character from dashing.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public void StopDashing()
        {
            // If not dashing, return

            if (!IsDashing())
                return;

            // Ends dashing state,
            // Re-Enable gravity, disable separate braking friction and clear dash impulse resenting character's velocity

            _isDashing = false;

            useSeparateBrakingFriction = false;
            brakingFriction = 0.0f;

            EnableGravity(true);
            SetVelocity(Vector3.zero);
        }

        /// <summary>
        /// Handle Dashing state.
        /// </summary>

        private void Dashing()
        {
            if (!IsDashing())
                return;

            // Update dashing time and if completed, stops dashing

            _dashingTime += deltaTime;
            if (_dashingTime > dashDuration)
                StopDashing();
        }

        /// <summary>
        /// Extends Move method to add dashing state.
        /// </summary>

        protected override void Move()
        {
            // Call base method implementation

            base.Move();

            // Dashing state

            Dashing();
        }

        /// <summary>
        /// Extends HandleInput method, to support dashing state.
        /// </summary>

        protected override void HandleInput()
        {
            // Call base method implementation

            base.HandleInput();

            // Dash input

            if (Input.GetKeyDown(KeyCode.E))
            {
                Dash();
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                StopDashing();
            }

            // If Dashing keep the character looking towards dashing direction

            if (IsDashing())
                SetMovementDirection(_dashDirection);
        }

        #endregion
    }
}
