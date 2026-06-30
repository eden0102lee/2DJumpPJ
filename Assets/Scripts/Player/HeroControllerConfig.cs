using UnityEngine;

namespace Hollow.Player
{
    [CreateAssetMenu(fileName = "HeroControllerConfig", menuName = "Hollow/Hero Controller Config")]
    public class HeroControllerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float runSpeed = 7.5f;
        public float acceleration = 60f;
        public float deceleration = 60f;
        public float airAcceleration = 40f;
        public float airDeceleration = 30f;

        [Header("Sprint")]
        public float sprintSpeed = 12f;
        public float sprintAcceleration = 80f;
        public float dashTapThreshold = 0.15f;

        [Header("Jump")]
        public float jumpForce = 15.5f;
        public float sprintJumpForce = 17f;
        public float sprintJumpHorizontalBoost = 1.3f;
        [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;
        public float coyoteTime = 0.12f;
        public float jumpBufferTime = 0.12f;
        public float maxFallSpeed = 25f;

        [Header("Wall")]
        public float wallSlideSpeed = 2f;
        public Vector2 wallJumpForce = new(11f, 15f);
        public float wallJumpLockTime = 0.15f;
        public float wallClingDelay = 0.08f;
        public float wallClimbSpeed = 5f;
        public Vector2 wallUpDashForce = new(0f, 16f);

        [Header("Dash")]
        public float dashSpeed = 22f;
        public float dashDuration = 0.16f;
        public float dashCooldown = 0.35f;
        public float dashIFrameDuration = 0.15f;
        [Range(0f, 1f)] public float dashEndMomentum = 0.55f;
        public bool allowAirDash = true;
        public bool allowDownDash = true;
        public float downDashSpeed = 18f;

        [Header("Physics")]
        public float gravityScale = 3f;
        public float fallGravityMultiplier = 1.8f;
        public float lowJumpGravityMultiplier = 2f;
    }
}
