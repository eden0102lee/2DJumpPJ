using UnityEngine;

namespace Hollow.Player
{
    public class HeroDownDashState : HeroBaseState
    {
        public HeroDownDashState(HeroController hero) : base(hero) { }

        public override void Enter()
        {
            Hero.StartIFrames();
            Rb.gravityScale = 0f;
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, -Config.downDashSpeed);
        }

        public override void Exit()
        {
            Rb.gravityScale = Config.gravityScale;
            Hero.StartDashCooldown();
        }

        public override void Tick()
        {
            if (!Hero.Input.DashHeld)
            {
                Hero.StateMachine.ChangeState(
                    Sensor.IsGrounded ? typeof(HeroIdleState) : typeof(HeroFallState));
            }
        }

        public override void FixedTick()
        {
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, -Config.downDashSpeed);
        }
    }
}
