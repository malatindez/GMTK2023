using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyCharacterMovement
{
    /// <summary>
    /// FirstPersonCharacter.
    ///
    /// This extends the Character class to add controls for a typical first person movement. 
    /// </summary>

    [RequireComponent(typeof(CharacterLook))]
    public class FirstPersonCharacter : Character
    {
        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("The first person rig root pivot. This handles the Yaw rotation.")]
        public Transform rootPivot;

        [Tooltip("The first person rig eye pivot. This handles the Pitch rotation.")]
        public Transform eyePivot;

        [Space(15f)]
        [Tooltip("The default eye height (eg: walking).")]
        [SerializeField]
        private float _eyeHeight;

        [Tooltip("The eye height while Character is crouched.")]
        [SerializeField]
        private float _eyeHeightCrouched;

        [Space(15f)]
        [Tooltip("The speed multiplier while Character is walking forward.")]
        [SerializeField]
        private float _forwardSpeedMultiplier;

        [Tooltip("The speed multiplier while Character is walking backward.")]
        [SerializeField]
        private float _backwardSpeedMultiplier;

        [Tooltip("The speed multiplier while Character is walking sideways.")]
        [SerializeField]
        private float _strafeSpeedMultiplier;
        
        #endregion

        #region FIELDS

        private CharacterLook _characterLook;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Cached CharacterLook component.
        /// </summary>

        protected CharacterLook characterLook
        {
            get
            {
                if (_characterLook == null)
                    _characterLook = GetComponent<CharacterLook>();

                return _characterLook;
            }
        }

        /// <summary>
        /// Default eye height (in meters).
        /// </summary>

        public float eyeHeight
        {
            get => _eyeHeight;
            set => _eyeHeight = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Default crouched eye height (in meters).
        /// </summary>

        public float eyeHeightCrouched
        {
            get => _eyeHeightCrouched;
            set => _eyeHeightCrouched = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The speed multiplier while Character is walking forward.
        /// </summary>

        public float forwardSpeedMultiplier
        {
            get => _forwardSpeedMultiplier;
            set => _forwardSpeedMultiplier = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The speed multiplier while Character is walking backwards.
        /// </summary>

        public float backwardSpeedMultiplier
        {
            get => _backwardSpeedMultiplier;
            set => _backwardSpeedMultiplier = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The speed multiplier while Character is strafing.
        /// </summary>

        public float strafeSpeedMultiplier
        {
            get => _strafeSpeedMultiplier;
            set => _strafeSpeedMultiplier = Mathf.Max(0.0f, value);
        }

        #endregion        

        #region METHODS        

        /// <summary>
        /// Return the CharacterLook component.
        /// This is guaranteed to be not null.
        /// </summary>

        public virtual CharacterLook GetCharacterLook()
        {
            return characterLook;
        }
        
        /// <summary>
        /// The Eye right vector.
        /// </summary>

        public virtual Vector3 GetEyeRightVector()
        {
            return eyePivot.right;
        }

        /// <summary>
        /// The Eye Up vector.
        /// </summary>

        public virtual Vector3 GetEyeUpVector()
        {
            return eyePivot.up;
        }

        /// <summary>
        /// The Eye forward vector.
        /// </summary>

        public virtual Vector3 GetEyeForwardVector()
        {
            return eyePivot.forward;
        }
        
        /// <summary>
        /// Look up / down. Adds Pitch rotation to eyePivot.
        /// </summary>

        public void AddEyePitchInput(float value)
        {
            if (value == 0.0f)
                return;

            eyePivot.localRotation *= Quaternion.Euler(characterLook.invertLook ? -value : value, 0.0f, 0.0f);

            if (characterLook.clampPitchRotation)
                eyePivot.localRotation = eyePivot.localRotation.clampPitch(characterLook.minPitchAngle, characterLook.maxPitchAngle);
        }

        /// <summary>
        /// The current speed multiplier based on movement direction,
        /// eg: walking forward, walking backwards or strafing side to side.
        /// </summary>

        protected virtual float GetSpeedModifier()
        {
            // Compute planar move direction

            Vector3 characterUp = GetUpVector();
            Vector3 planarMoveDirection = Vector3.ProjectOnPlane(GetMovementDirection(), characterUp);

            // Compute actual walk speed factoring movement direction

            Vector3 characterForward = Vector3.ProjectOnPlane(GetForwardVector(), characterUp).normalized;

            float forwardMovement = Vector3.Dot(planarMoveDirection, characterForward);

            float speedMultiplier = forwardMovement >= 0.0f
                ? Mathf.Lerp(strafeSpeedMultiplier, forwardSpeedMultiplier, forwardMovement)
                : Mathf.Lerp(strafeSpeedMultiplier, backwardSpeedMultiplier, -forwardMovement);

            return speedMultiplier;
        }

        /// <summary>
        /// The maximum speed for current movement mode, factoring walking movement direction.
        /// </summary>

        public override float GetMaxSpeed()
        {
            float actualMaxSpeed = base.GetMaxSpeed();

            return actualMaxSpeed * GetSpeedModifier();
        }

        /// <summary>
        /// Handles the character input.
        /// </summary>

        protected virtual void HandleCharacterInput()
        {
            // Movement

            Vector2 movementInput = new Vector2
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical"),
            };

            // Add input movement relative to us

            Vector3 movementDirection = Vector3.zero;

            movementDirection += GetRightVector() * movementInput.x;
            movementDirection += GetForwardVector() * movementInput.y;

            SetMovementDirection(movementDirection);

            // Jump

            if (Input.GetButtonDown("Jump"))
                Jump();
            else if (Input.GetButtonUp("Jump"))
                StopJumping();

            // Crouch

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
                Crouch();
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
                StopCrouching();

            // Sprint

            if (Input.GetKeyDown(KeyCode.LeftShift))
                Sprint();
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                StopSprinting();
        }

        /// <summary>
        /// Handle camera look input.
        /// </summary>

        protected virtual void HandleCameraInput()
        {
            // If Character is disabled, halts camera input

            if (IsDisabled())
                return;

            // Cursor lock / unlock

            if (Input.GetMouseButtonUp(0))
            {
                characterLook.LockCursor();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                characterLook.UnlockCursor();
            }

            if (!characterLook.IsCursorLocked())
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

            if (mouseLookInput.x != 0.0f)
                AddYawInput(mouseLookInput.x * characterLook.mouseHorizontalSensitivity);

            if (mouseLookInput.y != 0.0f)
                AddEyePitchInput(mouseLookInput.y * characterLook.mouseVerticalSensitivity);
        }

        /// <summary>
        /// Extends HandleInput method to add camera look input.
        /// </summary>

        protected override void HandleInput()
        {
            HandleCharacterInput();
            HandleCameraInput();
        }

        /// <summary>
        /// Helper method used to perform camera animation.
        /// Default implementation do basic procedural crouch animation.
        /// </summary>

        protected virtual void AnimateEye()
        {
            // Modify camera's height to simulate crouching state

            float actualEyeHeight = IsCrouching() ? eyeHeightCrouched : eyeHeight;

            eyePivot.localPosition =
                Vector3.MoveTowards(eyePivot.localPosition, new Vector3(0.0f, actualEyeHeight, 0.0f), 6.0f * Time.deltaTime);
        }

        /// <summary>
        /// Set this default values.
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected override void OnReset()
        {
            // Character defaults

            base.OnReset();

            // This defaults

            eyeHeight = 1.6f;
            eyeHeightCrouched = 1.0f;

            forwardSpeedMultiplier = 1.0f;
            backwardSpeedMultiplier = 0.5f;
            strafeSpeedMultiplier = 0.75f;

            SetRotationMode(RotationMode.None);
        }

        /// <summary>
        /// Validate editor exposed fields. 
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected override void OnOnValidate()
        {
            // Validates Character fields

            base.OnOnValidate();

            // Validate this

            eyeHeight = _eyeHeight;
            eyeHeightCrouched = _eyeHeightCrouched;

            forwardSpeedMultiplier = _forwardSpeedMultiplier;
            backwardSpeedMultiplier = _backwardSpeedMultiplier;
            strafeSpeedMultiplier = _strafeSpeedMultiplier;
        }

        /// <summary>
        /// Initialize this.
        /// </summary>

        protected override void OnAwake()
        {
            // Call base method

            base.OnAwake();

            // Disable Character rotation

            SetRotationMode(RotationMode.None);
        }

        /// <summary>
        /// Extends OnLateUpdate to perform procedural camera animation (i.e: crouch anim).
        /// </summary>

        protected override void OnLateUpdate()
        {
            // Is Character is disabled, return

            if (IsDisabled())
                return;

            AnimateEye();
        }

        #endregion
    }
}
