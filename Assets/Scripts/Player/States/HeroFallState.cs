using UnityEngine;

namespace Hollow.Player
{
    public class HeroFallState : HeroBaseState
    {
        public HeroFallState(HeroController hero) : base(hero) { }

        public override void Tick()
        {
            TryJump();
            TryTapDash();
            TryWallSlideTransition();

            if (!IsCurrentState<HeroFallState>())
                return;

            if (Sensor.IsGrounded)
            {
                Hero.StateMachine.ChangeState(
                    Mathf.Abs(MoveInput) > 0.01f ? typeof(HeroRunState) : typeof(HeroIdleState));
            }
        }

        public override void FixedTick()
        {
            ApplyHorizontalMovement(false);
            ApplyGravityModifiers();
        }
    }
}
