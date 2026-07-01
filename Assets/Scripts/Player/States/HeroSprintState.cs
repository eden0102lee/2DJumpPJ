using UnityEngine;

namespace Hollow.Player
{
    public class HeroSprintState : HeroBaseState
    {
        public HeroSprintState(HeroController hero) : base(hero) { }

        public override void Enter()
        {
            Hero.SetSprinting(true);
        }

        public override void Exit()
        {
            Hero.SetSprinting(false);
        }

        public override void Tick()
        {
            TryJump();
            TryTapDash();

            if (!IsCurrentState<HeroSprintState>())
                return;

            if (!Sensor.IsGrounded)
            {
                Hero.StateMachine.ChangeState(typeof(HeroFallState));
                return;
            }

            if (!Hero.Input.DashHeld || Mathf.Abs(MoveInput) <= 0.01f)
                Hero.StateMachine.ChangeState(typeof(HeroRunState));
        }

        public override void FixedTick()
        {
            ApplySprintMovement();
        }
    }
}
