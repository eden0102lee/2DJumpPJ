using UnityEngine;

namespace Hollow.Player
{
    public class HeroWallClimbState : HeroBaseState
    {
        public HeroWallClimbState(HeroController hero) : base(hero) { }

        public override void Enter()
        {
            var wallDir = Hero.GetActiveWallDirection();
            if (wallDir != 0)
                Hero.SetFacing(wallDir);
        }

        public override void Tick()
        {
            TryJump();
            TryTapDash();

            if (!IsCurrentState<HeroWallClimbState>())
                return;

            if (Sensor.IsGrounded)
            {
                Hero.StateMachine.ChangeState(typeof(HeroIdleState));
                return;
            }

            if (!Hero.ShouldTryWallClimb())
                Hero.StateMachine.ChangeState(typeof(HeroWallSlideState));
        }

        public override void FixedTick()
        {
            Rb.linearVelocity = new Vector2(0f, Config.wallClimbSpeed);
        }
    }
}
