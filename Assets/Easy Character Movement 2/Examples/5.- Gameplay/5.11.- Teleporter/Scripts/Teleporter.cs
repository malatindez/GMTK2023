using UnityEngine;

namespace EasyCharacterMovement.Examples.Gameplay.TeleporterExample
{
    /// <summary>
    /// This example shows how to teleport a character while (if desired) orient towards destination forward and preserve its momentum.
    /// </summary>

    public class Teleporter : MonoBehaviour
    {
        [Tooltip("The destination teleporter.")]
        public Teleporter destination;

        [Tooltip("If true, the character will orient towards the destination Teleporter forward (yaw only)")]
        public bool OrientWithDestination;

        /// <summary>
        /// Helps to prevent being instantly teleported back.
        /// </summary>

        public bool isTeleporterEnabled { get; set; } = true;

        private void OnTriggerEnter(Collider other)
        {
            // If no destination or this teleporter is disabled, return

            if (destination == null || !isTeleporterEnabled)
                return;

            if (other.TryGetComponent(out Character character))
            {
                // Teleport character

                character.TeleportPosition(destination.transform.position);
                
                // Disable destination teleporter until teleported character left it,
                // otherwise will be teleported back in an infinite loop!

                destination.isTeleporterEnabled = false;

                // Should orient with destination ?

                if (OrientWithDestination)
                {
                    // Update character's rotation towards destination forward vector (yaw only)

                    Vector3 characterUp = character.GetUpVector();
                    Vector3 teleporterForward = destination.transform.forward.projectedOnPlane(characterUp);

                    Quaternion newRotation = Quaternion.LookRotation(teleporterForward, character.GetUpVector());

                    character.TeleportRotation(newRotation);

                    // Re-orient character's horizontal velocity along teleporter forward

                    character.LaunchCharacter(teleporterForward * character.GetSpeed(), false, true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // On left, make sure teleporter is re-enabled

            isTeleporterEnabled = true;
        }
    }
}
