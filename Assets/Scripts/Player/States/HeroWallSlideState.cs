using UnityEngine;

namespace Hollow.Player
{
    public class HeroWallSlideState : HeroBaseState
    {
        private float _clingTimer;
        private bool _isClinging;

        public HeroWallSlideState(HeroController hero) : base(hero) { }

        public override void Enter()
        {
            var wallDir = Hero.GetActiveWallDirection();
            if (wallDir != 0)
                Hero.SetFacing(wallDir);

            _clingTimer = Config.wallClingDelay;
            _isClinging = true;
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, 0f);
        }

        public override void Tick()
        {
            TryWallClimbTransition();
            TryWallUpDash();
            TryJump();

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
            if (_isClinging)
            {
                _clingTimer -= Time.fixedDeltaTime;
                if (_clingTimer <= 0f)
                    _isClinging = false;

                Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, 0f);
                return;
            }

            var velocity = Rb.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, -Config.wallSlideSpeed);
            Rb.linearVelocity = velocity;
        }
    }
}
