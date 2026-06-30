using UnityEngine;

namespace Hollow.Player
{
    public class HeroIdleState : HeroBaseState
    {
        public HeroIdleState(HeroController hero) : base(hero) { }

        public override void Tick()
        {
            TryJump();
            TryTapDash();
            TryStartSprint();

            if (!IsCurrentState<HeroIdleState>())
                return;

            if (!Sensor.IsGrounded)
            {
                Hero.StateMachine.ChangeState(typeof(HeroFallState));
                return;
            }

            if (Mathf.Abs(MoveInput) > 0.01f)
                Hero.StateMachine.ChangeState(typeof(HeroRunState));
        }

        public override void FixedTick()
        {
            ApplyHorizontalMovement(true);
        }
    }
}
