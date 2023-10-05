using UnityEngine;

namespace EasyCharacterMovement.Examples.Animation.CharacterRootMotionToggleExample
{
    /// <summary>
    /// This example shows how to extend our Character to toggle root motion on/off based on its current movement mode (or a state).
    /// 
    /// In this example, we use the OnMovementModeChanged to enable / disable root motion on movement mode change.
    /// In this case, enable root motion when walking, otherwise disable it.
    /// 
    /// </summary>

    public class UnityCharacter : Character
    {
        private static readonly int ForwardId = Animator.StringToHash("Forward");
        private static readonly int TurnId = Animator.StringToHash("Turn");
        private static readonly int GroundId = Animator.StringToHash("OnGround");
        private static readonly int CrouchId = Animator.StringToHash("Crouch");
        private static readonly int JumpId = Animator.StringToHash("Jump");
        private static readonly int JumpLegId = Animator.StringToHash("JumpLeg");

        protected override void Animate()
        {
            float deltaTime = Time.deltaTime;

            // Get Character animator

            Animator animator = GetAnimator();

            // Compute input move vector in local space

            Vector3 move = transform.InverseTransformDirection(GetMovementDirection());

            // Update the animator parameters

            float forwardAmount = useRootMotion && GetRootMotionController()
                ? move.z
                : Mathf.InverseLerp(0.0f, GetMaxSpeed(), GetSpeed());

            animator.SetFloat(ForwardId, forwardAmount, 0.1f, deltaTime);
            animator.SetFloat(TurnId, Mathf.Atan2(move.x, move.z), 0.1f, deltaTime);

            animator.SetBool(GroundId, IsGrounded());
            animator.SetBool(CrouchId, IsCrouching());

            if (IsFalling())
                animator.SetFloat(JumpId, GetVelocity().y, 0.1f, deltaTime);

            // Calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)

            float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
            float jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

            if (IsGrounded())
                animator.SetFloat(JumpLegId, jumpLeg);
        }

        protected override void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            // Call base method implementation

            base.OnMovementModeChanged(prevMovementMode, prevCustomMode);

            // Toggle root motion, allow when walking, otherwise, disable it

            useRootMotion = IsWalking();
        }
    }
}
