using UnityEngine;

namespace EasyCharacterMovement.Examples.InputExamples.CustomInputExample
{
    /// <summary> 
    /// This shows how to extend the Character class and the steps needed to add custom input.
    /// </summary>

    public sealed class MyCharacter : Character
    {
        #region METHODS

        /// <summary>
        /// Start interaction.
        /// </summary>

        public void Interact()
        {
            Debug.Log("Player Pressed Interaction Button");
        }

        /// <summary>
        /// Stops interaction.
        /// </summary>

        public void StopInteracting()
        {
            Debug.Log("Player Released Interaction Button");
        }

        /// <summary>
        /// Extend HandleInput method to add new input based actions.
        /// </summary>

        protected override void HandleInput()
        {
            // Handle default inputs

            base.HandleInput();

            // Add interaction input

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();

            } else if (Input.GetKeyUp(KeyCode.E))
            {
                StopInteracting();
            }
        }

        #endregion
    }
}
