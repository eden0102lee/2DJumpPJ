using UnityEngine;

namespace Hollow.Player
{
    [CreateAssetMenu(fileName = "HeroControllerConfig", menuName = "Hollow/Hero Controller Config")]
    public class HeroControllerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float runSpeed = 8f;
        public float acceleration = 60f;
        public float deceleration = 60f;
        public float airAcceleration = 40f;
        public float airDeceleration = 30f;

        [Header("Jump")]
        public float jumpForce = 14f;
        [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;
        public float coyoteTime = 0.12f;
        public float jumpBufferTime = 0.12f;
        public float maxFallSpeed = 25f;

        [Header("Wall")]
        public float wallSlideSpeed = 2f;
        public Vector2 wallJumpForce = new(10f, 14f);
        public float wallJumpLockTime = 0.2f;

        [Header("Dash")]
        public float dashSpeed = 20f;
        public float dashDuration = 0.18f;
        public float dashCooldown = 0.5f;
        public bool allowAirDash = true;

        [Header("Physics")]
        public float gravityScale = 3f;
        public float fallGravityMultiplier = 1.5f;
        public float lowJumpGravityMultiplier = 2f;
    }
}
