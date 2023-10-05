using UnityEngine;

namespace EasyCharacterMovement.Examples.Gameplay.TwinStickExample
{
    /// <summary>
    /// This example shows how to extend the Character class to perform a typical twin-stick shooter movement,
    /// where the character is moved with left stick and aim with the right stick. 
    /// </summary>

    public class TwinStickCharacter : Character
    {
        #region FIELDS

        private Vector2 _fireInput;

        #endregion

        #region METHODS

        /// <summary>
        /// Override UpdateRotation method to add support for right stick aim rotation.
        /// </summary>

        protected override void UpdateRotation()
        {
            Vector2 mousePosition = Input.mousePosition;

            Ray ray = camera.ScreenPointToRay(mousePosition);

            LayerMask groundMask = characterMovement.collisionLayers;

            QueryTriggerInteraction triggerInteraction = characterMovement.triggerInteraction;

            if (Physics.Raycast(ray, out RaycastHit hitResult, Mathf.Infinity, groundMask, triggerInteraction))
            {
                // Rotate towards mouse position (in world)

                Vector3 aimDirection = (hitResult.point - transform.position).onlyXZ().normalized;

                RotateTowardsWithSlerp(aimDirection);
            }
            else
            {
                // Rotate towards movement direction

                Vector3 movementDirection = GetMovementDirection();

                RotateTowardsWithSlerp(movementDirection);
            }
        }

        #endregion
    }
}
