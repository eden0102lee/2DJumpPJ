using UnityEngine;

namespace Hollow.Player
{
    public abstract class HeroBaseState : IHeroState
    {
        protected readonly HeroController Hero;
        protected readonly HeroControllerConfig Config;
        protected readonly Rigidbody2D Rb;
        protected readonly GroundWallSensor Sensor;

        protected HeroBaseState(HeroController hero)
        {
            Hero = hero;
            Config = hero.Config;
            Rb = hero.Rigidbody;
            Sensor = hero.Sensor;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick() { }
        public virtual void FixedTick() { }

        protected float MoveInput => Hero.GetEffectiveMoveInput();

        protected void ApplyHorizontalMovement(bool grounded)
        {
            if (!Hero.CanMove)
                return;

            var targetSpeed = MoveInput * Config.runSpeed;
            var accel = grounded ? Config.acceleration : Config.airAcceleration;
            var decel = grounded ? Config.deceleration : Config.airDeceleration;
            var rate = Mathf.Abs(targetSpeed) > 0.01f ? accel : decel;

            var newX = Mathf.MoveTowards(Rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);
            Rb.linearVelocity = new Vector2(newX, Rb.linearVelocity.y);
        }

        protected void ApplyGravityModifiers()
        {
            if (Sensor.IsGrounded && Rb.linearVelocity.y <= 0f)
                return;

            var gravityScale = Config.gravityScale;

            if (Rb.linearVelocity.y < 0f)
                gravityScale *= Config.fallGravityMultiplier;
            else if (Rb.linearVelocity.y > 0f && !Hero.Input.JumpHeld)
                gravityScale *= Config.lowJumpGravityMultiplier;

            Rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (gravityScale - 1f) * Rb.gravityScale) * Time.fixedDeltaTime;

            if (Rb.linearVelocity.y < -Config.maxFallSpeed)
                Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, -Config.maxFallSpeed);
        }

        protected void TryJump()
        {
            Hero.TryConsumeJump();
        }

        protected void TryDash()
        {
            if (!Hero.Input.DashPressed)
                return;

            if (Hero.TryStartDash())
                Hero.StateMachine.ChangeState(typeof(HeroDashState));
        }

        protected bool IsCurrentState<T>() where T : IHeroState
        {
            return Hero.StateMachine.CurrentStateType == typeof(T);
        }

        protected void TryWallSlideTransition()
        {
            if (Hero.ShouldWallSlide())
            {
                Hero.StateMachine.ChangeState(typeof(HeroWallSlideState));
            }
        }
    }
}
