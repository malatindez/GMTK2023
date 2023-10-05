using EasyCharacterMovement;
using UnityEngine;

namespace EasyCharacterMovement.Examples.Demo
{
    public class ApplyLandingForce : MonoBehaviour
    {
        public float LandingForceScale = 1.0f;

        private Character _character;

        private void OnCharacterLanded()
        {
            CharacterMovement characterMovement = _character.GetCharacterMovement();

            Rigidbody rb = characterMovement.groundRigidbody;
            if (rb && !rb.isKinematic)
            {
                rb.AddForceAtPosition(
                    LandingForceScale * _character.gravity.magnitude * characterMovement.landedVelocity,
                    characterMovement.groundPoint);
            }
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
            _character.Landed += OnCharacterLanded;
        }
    }
}
