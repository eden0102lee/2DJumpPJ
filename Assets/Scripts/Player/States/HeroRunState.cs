using UnityEngine;

namespace Hollow.Player
{
    public class HeroRunState : HeroBaseState
    {
        public HeroRunState(HeroController hero) : base(hero) { }

        public override void Tick()
        {
            TryJump();
            TryDash();
            if (!IsCurrentState<HeroRunState>())
                return;

            if (!Sensor.IsGrounded)
            {
                Hero.StateMachine.ChangeState(typeof(HeroFallState));
                return;
            }

            if (Mathf.Abs(MoveInput) <= 0.01f)
                Hero.StateMachine.ChangeState(typeof(HeroIdleState));
        }

        public override void FixedTick()
        {
            ApplyHorizontalMovement(true);
        }
    }
}
