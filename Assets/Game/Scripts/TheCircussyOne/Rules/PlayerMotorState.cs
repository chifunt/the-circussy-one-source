using UnityEngine;

namespace TheCircussyOne.Rules
{
    public sealed class PlayerMotorState
    {
        public Vector3 Velocity { get; set; }
        public float VerticalVelocity { get; set; }
        public Vector3 Facing { get; set; } = Vector3.forward;
        public bool IsSkidding { get; set; }
        public bool IsGrounded { get; set; } = true;
        public bool WasGrounded { get; set; } = true;
        public int JumpsUsed { get; set; }
        public float CoyoteTimer { get; set; }
        public float JumpBufferTimer { get; set; }
        public float AirTime { get; set; }
        public bool JumpStartedThisFrame { get; set; }
        public bool LandedThisFrame { get; set; }

        public Vector3 FullVelocity => Velocity + Vector3.up * VerticalVelocity;
    }
}
