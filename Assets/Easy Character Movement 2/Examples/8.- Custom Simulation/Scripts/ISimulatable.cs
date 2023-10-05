
namespace EasyCharacterMovement.CharacterMovementExamples
{
    public interface ISimulatable
    {
        void PrePhysicsUpdate(float deltaTime);

        void PostPhysicsUpdate(float deltaTime);

        void Interpolate(float interpolationFactor);
    }
}
