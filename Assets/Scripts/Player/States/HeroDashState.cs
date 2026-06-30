using UnityEngine;

namespace Hollow.Player
{
    public class HeroDashState : HeroBaseState
    {
        private float _dashTimer;
        private int _dashDirection;

        public HeroDashState(HeroController hero) : base(hero) { }

        public override void Enter()
        {
            _dashTimer = Config.dashDuration;
            _dashDirection = Hero.FacingDirection;
            if (Mathf.Abs(Hero.Input.MoveInput.x) > 0.01f)
                _dashDirection = Hero.Input.MoveInput.x > 0f ? 1 : -1;

            Hero.SetFacing(_dashDirection);
            Rb.gravityScale = 0f;
            Rb.linearVelocity = new Vector2(Config.dashSpeed * _dashDirection, 0f);
        }

        public override void Exit()
        {
            Rb.gravityScale = Config.gravityScale;
            Hero.StartDashCooldown();
        }

        public override void Tick()
        {
            _dashTimer -= Time.deltaTime;

            if (_dashTimer <= 0f)
            {
                Hero.StateMachine.ChangeState(
                    Sensor.IsGrounded ? typeof(HeroIdleState) : typeof(HeroFallState));
            }
        }

        public override void FixedTick()
        {
            Rb.linearVelocity = new Vector2(Config.dashSpeed * _dashDirection, 0f);
        }
    }
}
