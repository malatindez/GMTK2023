using UnityEngine;

namespace EasyCharacterMovement.CharacterMovementExamples
{
    /// <summary>
    /// This example shows how modify a Character to comply with custom simulation.
    /// </summary>

    public class MySimulatedCharacter : ThirdPersonCharacter, ISimulatable
    {
        #region FIELDS

        private Vector3 _lastUpdatedPosition;
        private Quaternion _lastUpdatedRotation;

        #endregion

        #region ISimulateble IMPLEMENTATION

        public void PrePhysicsUpdate(float deltaTime)
        {
            // EMPTY as a Character must be simulated AFTER physics update if requires physics integration,
            // or after your dynamic platforms
        }

        public void PostPhysicsUpdate(float deltaTime)
        {
            // Save pre-simulation poses,
            // and make sure the character is at its up-to-date position and rotation (NOT INTERPOLATED ONES) before simulate it

            _lastUpdatedPosition = characterMovement.updatedPosition;
            _lastUpdatedRotation = characterMovement.updatedRotation;

            characterMovement.SetPositionAndRotation(characterMovement.updatedPosition, characterMovement.updatedRotation);
            
            // Simulate this Character, e.g. update it

            Simulate(deltaTime);
        }

        /// <summary>
        /// Interpolate character pose, this is only needed when using custom simulation with FIXED timestep.
        /// </summary>

        public void Interpolate(float interpolationFactor)
        {
            // Set transform's interpolated pose

            Vector3 p = Vector3.Slerp(_lastUpdatedPosition, characterMovement.updatedPosition, interpolationFactor);
            Quaternion q = Quaternion.Slerp(_lastUpdatedRotation, characterMovement.updatedRotation, interpolationFactor);

            transform.SetPositionAndRotation(p, q);
        }

        #endregion

        #region METHODS

        protected override void OnAwake()
        {
            // Register this with Simulation Manager

            base.OnAwake();

            // Disable LateFixedUpdate coroutine as this will be handled by SimulationManager

            enableLateFixedUpdate = false;
        }

        protected override void OnOnEnable()
        {
            // Call base method implementation

            base.OnOnEnable();

            // Register this with SimulationManager

            SimulationManager.instance.AddSimulatable(this);
        }

        protected override void OnOnDisable()
        {
            // Call base method implementation

            base.OnOnDisable();

            // Un-Register this from SimulationManager

            SimulationManager.instance.RemoveSimulatable(this);
        }

        #endregion
    }
}
