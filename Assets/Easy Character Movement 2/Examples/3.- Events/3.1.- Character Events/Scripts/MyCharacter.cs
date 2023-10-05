using UnityEngine;

namespace EasyCharacterMovement.Examples.Events.CharacterEventsExample
{
    /// <summary>
    /// This shows how to extend the Character 'On' event handlers to respond to Character specific events like: landed, jumped, etc.
    /// Its important when extending a OnXXX event handler, ALWAYS call its base.OnXXX first, as this is the responsible of actually trigger the Event.
    /// </summary>

    public class MyCharacter : Character
    {
        #region EVENTS

        // DEPRECATED, replaced by OnCollided

        //protected override void OnMovementHit(ref MovementHit movementHitResult)
        //{
        //    // Call base method implementation

        //    base.OnMovementHit(ref movementHitResult);

        //    Debug.Log("Movement Hit " + movementHitResult.collider.name);
        //}

        // DEPRECATED, replaced by OnFoundGround

        //protected override void OnGroundHit(ref GroundHit prevGroundHitResult, ref GroundHit groundHitResult)
        //{
        //    // Call base method implementation

        //    base.OnGroundHit(ref prevGroundHitResult, ref groundHitResult);
            
        //    // Commented as this will spam the console

        //    // Debug.Log("Hit Ground " + groundHitResult.transform.name);
        //}
        
        protected override void OnCollided(ref CollisionResult collisionResult)
        {
            // Call base method implementation

            base.OnCollided(ref collisionResult);

            Debug.Log("Collided with " + collisionResult.collider.name);
        }

        protected override void OnFoundGround(ref FindGroundResult foundGround)
        {
            // Call base method implementation

            base.OnFoundGround(ref foundGround);

            Debug.Log("Found " + foundGround.collider.name + " ground.");
        }

        protected override void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            // Call base method implementation

            base.OnMovementModeChanged(prevMovementMode, prevCustomMode);

            Debug.Log("Changed from " + prevMovementMode + " to " + GetMovementMode());
        }

        protected override void OnJumped()
        {
            // Call base method implementation

            base.OnJumped();

            // Enable jump apex event notification, otherwise wont receive ReachedJumpApex Event

            notifyJumpApex = true;

            Debug.Log("Jump!");
        }

        protected override void OnReachedJumpApex()
        {
            // Call base method implementation

            base.OnReachedJumpApex();

            Debug.Log("Reached jump apex at " + fallingTime + " seconds");
        }

        protected override void OnWillLand()
        {
            // Call base method implementation

            base.OnWillLand();

            Debug.Log("The Character is about to land");
        }

        protected override void OnLanded()
        {
            // Call base method implementation

            base.OnLanded();

            Debug.Log("Landed! with a terminal velocity of" + characterMovement.landedVelocity);
        }

        protected override void OnCrouched()
        {
            // Call base method implementation

            base.OnCrouched();

            Debug.Log("The Character has crouched");
        }

        protected override void OnUnCrouched()
        {
            // Call base method implementation

            base.OnUnCrouched();

            Debug.Log("The Character has UnCrouched");
        }

        protected override void OnSprinted()
        {
            // Call base method implementation

            base.OnSprinted();

            Debug.Log("The Character has sprinted!");
        }

        protected override void OnStoppedSprinting()
        {
            // Call base method implementation

            base.OnStoppedSprinting();

            Debug.Log("The Character has stopped sprinting");
        }

        #endregion
    }
}
