using UnityEngine;

namespace Hollow.Player
{
    public class HeroWallSlideState : HeroBaseState
    {
        public HeroWallSlideState(HeroController hero) : base(hero) { }

        public override void Enter()
        {
            Hero.SetFacing(Sensor.WallDirection);
        }

        public override void Tick()
        {
            TryJump();
            TryDash();
            if (!IsCurrentState<HeroWallSlideState>())
                return;

            if (Sensor.IsGrounded)
            {
                Hero.StateMachine.ChangeState(typeof(HeroIdleState));
                return;
            }

            if (!Hero.ShouldWallSlide())
                Hero.StateMachine.ChangeState(typeof(HeroFallState));
        }

        public override void FixedTick()
        {
            var velocity = Rb.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, -Config.wallSlideSpeed);
            Rb.linearVelocity = velocity;
        }
    }
}
