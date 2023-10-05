using UnityEngine;

namespace EasyCharacterMovement.Examples.Gameplay.LadderClimbExample
{
    /// <summary>
    /// This Character's current custom movement mode.
    /// </summary>

    public enum NewMovementMode
    {
        Climbing = 1
    }

    /// <summary>
    /// The Character's custom movement mode states.
    /// </summary>
    
    public enum ClimbingState
    {
        None,
        Grabbing,
        Grabbed,
        Releasing
    }

    /// <summary>
    /// This example shows to add a custom movement mode to a Character.
    /// In this case, we add a ladder climb movement mode with different states for this Climbing movement mode.
    /// </summary>

    public class MyCharacter : Character
    {
        #region EDITOR EXPOSED FIELDS

        [Header("Climbing")]
        public float climbingSpeed = 5.0f;
        public float grabbingTime = 0.25f;

        public LayerMask ladderMask;

        #endregion

        #region FIELDS

        private Ladder _activeLadder;
        private float _ladderPathPosition;

        private Vector3 _ladderStartPosition;
        private Vector3 _ladderTargetPosition;

        private Quaternion _ladderStartRotation;
        private Quaternion _ladderTargetRotation;

        private float _ladderTime;

        private ClimbingState _climbingState;

        #endregion

        #region METHODS

        /// <summary>
        /// Extends OnMovementModeChanged to handle our custom movement mode enter / exit.
        /// </summary>

        protected override void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            // Call base method implementation

            base.OnMovementModeChanged(prevMovementMode, prevCustomMode);

            // Entering climbing movement mode

            if (_movementMode == MovementMode.Custom && _customMovementMode == (int) NewMovementMode.Climbing)
            {
                StopJumping();

                characterMovement.constrainToGround = false;
            }

            // Leaving climbing movement mode 

            if (prevCustomMode == (int) NewMovementMode.Climbing)
            {
                _climbingState = ClimbingState.None;

                characterMovement.constrainToGround = true;
            }
        }

        /// <summary>
        /// Is the Character in climbing movement mode ?
        /// </summary>

        public bool IsClimbing()
        {
            return _movementMode == MovementMode.Custom && _customMovementMode == (int) NewMovementMode.Climbing;
        }

        /// <summary>
        /// Determines if the Character is able to climb.
        /// </summary>

        private bool CanClimb()
        {
            // Do not allow to climb if crouching

            if (IsCrouching())
                return false;

            // Fin a ladder to climb...

            var overlappedColliders = characterMovement.OverlapTest(GetPosition(), GetRotation(), GetRadius(),
                GetHeight(), ladderMask, QueryTriggerInteraction.Collide, out int overlapCount);

            if (overlapCount == 0)
                return false;

            // Is a ladder ?

            if (!overlappedColliders[0].TryGetComponent(out Ladder ladder))
                return false;

            // Found a ladder, make it active ladder and return
            
            _activeLadder = ladder;

            return true;
        }

        /// <summary>
        /// Start a climb.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public void Climb()
        {
            if (IsClimbing() || !CanClimb())
                return;

            SetMovementMode(MovementMode.Custom, (int) NewMovementMode.Climbing);
            
            _climbingState = ClimbingState.Grabbing;

            _ladderStartPosition = GetPosition();
            _ladderTargetPosition = _activeLadder.ClosestPointOnPath(_ladderStartPosition, out _ladderPathPosition);

            _ladderStartRotation = GetRotation();
            _ladderTargetRotation = _activeLadder.transform.rotation;
        }

        /// <summary>
        /// Stop the Character from climbing.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public void StopClimbing()
        {
            if (!IsClimbing() || _climbingState != ClimbingState.Grabbed)
                return;

            _climbingState = ClimbingState.Releasing;

            _ladderStartPosition = GetPosition();
            _ladderStartRotation = GetRotation();

            _ladderTargetPosition = _ladderStartPosition;
            _ladderTargetRotation = _activeLadder.BottomPoint.rotation;
        }

        /// <summary>
        /// Perform climbing movement.
        /// </summary>

        private void Climbing()
        {
            Vector3 velocity = Vector3.zero;

            switch (_climbingState)
            {
                case ClimbingState.Grabbing:
                case ClimbingState.Releasing:
                {
                    _ladderTime += Time.deltaTime;

                    if (_ladderTime <= grabbingTime)
                    {
                        Vector3 interpolatedPosition = Vector3.Lerp(_ladderStartPosition, _ladderTargetPosition, _ladderTime / grabbingTime);
                    
                        velocity = (interpolatedPosition - transform.position) / Time.deltaTime;
                    }
                    else
                    {
                        // If target has been reached, change ladder phase

                        _ladderTime = 0.0f;

                        if (_climbingState == ClimbingState.Grabbing )
                        {
                            // Switch to ladder climb phase

                            _climbingState = ClimbingState.Grabbed;
                        }
                        else if (_climbingState == ClimbingState.Releasing)
                        {
                            // Exit climbing state (change to falling movement mode)

                            SetMovementMode(MovementMode.Falling);
                        }
                    }

                    break;
                }

                case ClimbingState.Grabbed:
                {
                    // Get the path position from character's current position

                    _activeLadder.ClosestPointOnPath(GetPosition(), out _ladderPathPosition);

                    if (Mathf.Abs(_ladderPathPosition) < 0.05f)
                    {
                            // Move the character along the ladder path

                            Vector2 movementInput = new Vector2
                            {
                                x = Input.GetAxisRaw("Horizontal"),
                                y = Input.GetAxisRaw("Vertical")
                            };

                            velocity = _activeLadder.transform.up * movementInput.y * climbingSpeed;
                    }
                    else
                    {
                        // If reached on of the ladder path extremes, change to releasing phase

                        _climbingState = ClimbingState.Releasing;

                        _ladderStartPosition = GetPosition();
                        _ladderStartRotation = GetRotation();

                        if (_ladderPathPosition > 0.0f)
                        {
                            // Above ladder path top point

                            _ladderTargetPosition = _activeLadder.TopPoint.position;
                            _ladderTargetRotation = _activeLadder.TopPoint.rotation;
                        }
                        else if (_ladderPathPosition < 0.0f)
                        {
                            // Below ladder path bottom point

                            _ladderTargetPosition = _activeLadder.BottomPoint.position;
                            _ladderTargetRotation = _activeLadder.BottomPoint.rotation;
                        }
                    }

                    break;
                }
            }

            // Update character's velocity

            characterMovement.velocity = velocity;
        }

        /// <summary>
        /// Apply a custom movement mode, in this case ladder climbing.
        /// I.e: Moving along a ladder path.
        /// </summary>

        protected override void CustomMovementMode(Vector3 desiredVelocity)
        {
            if (_customMovementMode == (int) NewMovementMode.Climbing)
                Climbing();
        }

        /// <summary>
        /// Extend HandleInput method to add ladder interaction
        /// </summary>

        protected override void HandleInput()
        {
            base.HandleInput();

            // Ladder interaction input

            if (Input.GetKeyDown(KeyCode.E))
            {
                Climb();
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                StopClimbing();
            }
        }

        /// <summary>
        /// Extends UpdateRotation method to handle ladder climbing state.
        /// </summary>

        protected override void UpdateRotation()
        {
            if (!IsClimbing())
                base.UpdateRotation();
            else if (_climbingState == ClimbingState.Grabbing || _climbingState == ClimbingState.Releasing)
            {
                // Align to ladder

                characterMovement.rotation = Quaternion.Slerp(_ladderStartRotation, _ladderTargetRotation, _ladderTime / grabbingTime);
            }
        }

        #endregion
    }
}
