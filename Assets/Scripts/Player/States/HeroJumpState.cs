using UnityEngine;

namespace Hollow.Player
{
    public class HeroJumpState : HeroBaseState
    {
        public HeroJumpState(HeroController hero) : base(hero) { }

        public override void Enter()
        {
            if (Hero.CameFromSprint)
                Hero.PerformSprintJump();
            else
                Hero.PerformJump(Config.jumpForce);
        }

        public override void Exit()
        {
            Hero.SetSprinting(false);
        }

        public override void Tick()
        {
            TryTapDash();
            Hero.ApplyVariableJumpCut();
            TryWallSlideTransition();

            if (!IsCurrentState<HeroJumpState>())
                return;

            if (Rb.linearVelocity.y <= 0f)
                Hero.StateMachine.ChangeState(typeof(HeroFallState));
        }

        public override void FixedTick()
        {
            ApplyHorizontalMovement(false);
            ApplyGravityModifiers();
        }
    }
}
