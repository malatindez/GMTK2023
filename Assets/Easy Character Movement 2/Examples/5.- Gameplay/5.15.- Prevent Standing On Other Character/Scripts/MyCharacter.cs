using UnityEngine;

namespace EasyCharacterMovement.Examples.PreventStandingOnOtherCharacterExample
{
    /// <summary>
    /// This example shows how to force a character to slide off when standing on top of other character.
    /// </summary>

    public class MyCharacter : Character
    {
        private void SlideOffCharacters()
        {
            // Standing over other Character ?

            if (characterMovement.isOnGround && characterMovement.isConstrainedToGround)
            {
                Rigidbody otherRigidbody = characterMovement.groundRigidbody;

                if (otherRigidbody && otherRigidbody.TryGetComponent(out Character _))
                {
                    // Yes, slide off it!

                    Vector3 n = characterMovement.groundNormal;
                    Vector3 r = n.perpendicularTo(GetUpVector());
                    Vector3 slideDirection = n.perpendicularTo(r);

                    Debug.DrawRay(characterMovement.groundPoint, n, Color.green, 1f);
                    Debug.DrawRay(characterMovement.groundPoint, r, Color.red, 1f);
                    Debug.DrawRay(characterMovement.groundPoint, slideDirection, Color.blue, 1f);

                    PauseGroundConstraint();
                    LaunchCharacter(slideDirection * jumpImpulse * 0.5f);
                }
            }
        }

        protected override void Move()
        {
            // Call base implementation

            base.Move();

            // Apply slide force

            SlideOffCharacters();
        }
    }
}
