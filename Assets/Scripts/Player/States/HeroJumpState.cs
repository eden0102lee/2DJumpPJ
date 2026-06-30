using UnityEngine;

namespace Hollow.Player
{
    public class HeroJumpState : HeroBaseState
    {
        public HeroJumpState(HeroController hero) : base(hero) { }

        public override void Enter()
        {
            Hero.PerformJump(Config.jumpForce);
        }

        public override void Tick()
        {
            TryDash();
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
