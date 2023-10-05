using UnityEngine;

namespace EasyCharacterMovement.Examples.Events.CharacterControllerEventsExample
{
    /// <summary>
    /// This example shows how a Controller can subscribe to its controlled Character events.
    /// 
    /// </summary>

    public class MyCharacterController : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [SerializeField]
        public Camera _camera;

        [SerializeField]
        public Character _character;

        #endregion

        #region CHARACTER EVENTS HANDLERS
        
        private void OnFoundGround(ref FindGroundResult foundGround)
        {
            Debug.Log("The Character has found the ground " + foundGround.collider.name);
        }

        private void OnCollided(ref CollisionResult collisionResult)
        {
            Debug.Log("The Character has collided with " + collisionResult.collider.name);
        }

        private void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            Debug.Log("Changed from " + prevMovementMode + " to " + _character.GetMovementMode());
        }
        
        private void OnSprinted()
        {
            Debug.Log("The Character has sprinted.");
        }

        private void OnStoppedSprinting()
        {
            Debug.Log("The Character has stopped sprinting.");
        }

        private void OnCrouched()
        {
            Debug.Log("The Character has crouched.");
        }

        private void OnUnCrouched()
        {
            Debug.Log("The Character has UnCrouched.");
        }

        private void OnJumped()
        {
            Debug.Log("Jump!");

            // Enable jump apex event notification, otherwise wont receive ReachedJumpApex Event

            _character.notifyJumpApex = true;
        }

        private void OnReachedJumpApex()
        {
            Debug.Log("Reached jump apex at " + _character.fallingTime + " seconds");
        }

        private void OnWillLand()
        {
            Debug.Log("The Character is about to land");
        }

        private void OnLanded()
        {
            CharacterMovement characterMovement = _character.GetCharacterMovement();

            Debug.Log("Landed! with a terminal velocity of" + characterMovement.landedVelocity);
        }

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Subscribe to controlled Character's events.
        /// </summary>

        private void OnEnable()
        {
            // CharacterMovement component events

            if (_character == null)
                return;

            CharacterMovement characterMovement = _character.GetCharacterMovement();

            characterMovement.Collided += OnCollided;
            characterMovement.FoundGround += OnFoundGround;

            // Character events

            _character.MovementModeChanged += OnMovementModeChanged;
            
            _character.Sprinted += OnSprinted;
            _character.StoppedSprinting += OnStoppedSprinting;

            _character.Crouched += OnCrouched;  
            _character.UnCrouched += OnUnCrouched;

            _character.Jumped += OnJumped;
            _character.ReachedJumpApex += OnReachedJumpApex;

            _character.WillLand += OnWillLand;
            _character.Landed += OnLanded;
        }

        /// <summary>
        /// Unsubscribe to controlled Character's events.
        /// </summary>

        private void OnDisable()
        {
            // CharacterMovement component events

            if (_character == null)
                return;

            CharacterMovement characterMovement = _character.GetCharacterMovement();

            characterMovement.Collided -= OnCollided;
            characterMovement.FoundGround -= OnFoundGround;

            // Character events

            _character.MovementModeChanged -= OnMovementModeChanged;
            
            _character.Sprinted -= OnSprinted;
            _character.StoppedSprinting -= OnStoppedSprinting;

            _character.Crouched -= OnCrouched;  
            _character.UnCrouched -= OnUnCrouched;

            _character.Jumped -= OnJumped;
            _character.ReachedJumpApex -= OnReachedJumpApex;

            _character.WillLand -= OnWillLand;
            _character.Landed -= OnLanded;
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

            if (_camera != null)
            {
                movementDirection = movementDirection.relativeTo(_camera.transform);
            }

            _character.SetMovementDirection(movementDirection);

            // Jump

            if (Input.GetButton("Jump"))
            {
                _character.Jump();
            }
            else if (Input.GetButtonUp("Jump"))
            {
                _character.StopJumping();
            }

            // Crouch 

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
            {
                _character.Crouch();
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
            {
                _character.StopCrouching();
            }

            // Sprint

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _character.Sprint();
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                _character.StopSprinting();
            }
        }

        #endregion
    }
}
