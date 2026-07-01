using UnityEngine;

namespace Hollow.Player
{
    [RequireComponent(typeof(Animator))]
    public class HeroAnimator : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int VelocityY = Animator.StringToHash("VelocityY");
        private static readonly int IsWallSliding = Animator.StringToHash("IsWallSliding");
        private static readonly int IsDashing = Animator.StringToHash("IsDashing");
        private static readonly int IsSprinting = Animator.StringToHash("IsSprinting");
        private static readonly int IsWallClimbing = Animator.StringToHash("IsWallClimbing");
        private static readonly int IsDownDashing = Animator.StringToHash("IsDownDashing");

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void UpdateAnimator(HeroController hero)
        {
            if (_animator == null)
                return;

            var rb = hero.Rigidbody;
            var stateType = hero.StateMachine.CurrentStateType;

            _animator.SetFloat(Speed, Mathf.Abs(rb.linearVelocity.x));
            _animator.SetBool(IsGrounded, hero.Sensor.IsGrounded);
            _animator.SetFloat(VelocityY, rb.linearVelocity.y);
            _animator.SetBool(IsWallSliding, stateType == typeof(HeroWallSlideState));
            _animator.SetBool(IsDashing, stateType == typeof(HeroDashState));
            _animator.SetBool(IsSprinting, stateType == typeof(HeroSprintState));
            _animator.SetBool(IsWallClimbing, stateType == typeof(HeroWallClimbState));
            _animator.SetBool(IsDownDashing, stateType == typeof(HeroDownDashState));

            if (_spriteRenderer != null && hero.FacingDirection != 0)
            {
                _spriteRenderer.flipX = hero.FacingDirection < 0;
            }
        }
    }
}
