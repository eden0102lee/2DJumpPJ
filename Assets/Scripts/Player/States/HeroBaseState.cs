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

        protected void ApplyHorizontalMovement(bool grounded, float targetSpeed = -1f)
        {
            if (!Hero.CanMove)
                return;

            if (targetSpeed < 0f)
                targetSpeed = Config.runSpeed;

            var target = MoveInput * targetSpeed;
            var accel = grounded ? Config.acceleration : Config.airAcceleration;
            var decel = grounded ? Config.deceleration : Config.airDeceleration;
            var rate = Mathf.Abs(target) > 0.01f ? accel : decel;

            var newX = Mathf.MoveTowards(Rb.linearVelocity.x, target, rate * Time.fixedDeltaTime);
            Rb.linearVelocity = new Vector2(newX, Rb.linearVelocity.y);
        }

        protected void ApplySprintMovement()
        {
            if (!Hero.CanMove)
                return;

            var target = MoveInput * Config.sprintSpeed;
            var rate = Mathf.Abs(target) > 0.01f ? Config.sprintAcceleration : Config.deceleration;
            var newX = Mathf.MoveTowards(Rb.linearVelocity.x, target, rate * Time.fixedDeltaTime);
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

        protected void TryTapDash()
        {
            if (Hero.ShouldTryDownDash())
            {
                if (Hero.ShouldTryAirDash() && Hero.TryStartDash())
                    Hero.StateMachine.ChangeState(typeof(HeroDownDashState));
                return;
            }

            if (Sensor.IsGrounded)
            {
                if (Hero.ShouldTryGroundDash() && Hero.TryStartDash())
                    Hero.StateMachine.ChangeState(typeof(HeroDashState));
            }
            else if (Hero.ShouldTryAirDash() && Hero.TryStartDash())
            {
                Hero.StateMachine.ChangeState(typeof(HeroDashState));
            }
        }

        protected void TryStartSprint()
        {
            if (!Hero.ShouldStartSprint())
                return;

            Hero.StateMachine.ChangeState(typeof(HeroSprintState));
        }

        protected void TryWallUpDash()
        {
            if (!IsCurrentState<HeroWallSlideState>())
                return;

            if (!Hero.ShouldTryWallUpDash())
                return;

            if (!Hero.TryStartDash())
                return;

            Hero.PerformWallUpDash();
            Hero.StateMachine.ChangeState(typeof(HeroFallState));
        }

        protected void TryWallClimbTransition()
        {
            if (!Hero.ShouldTryWallClimb())
                return;

            Hero.StateMachine.ChangeState(typeof(HeroWallClimbState));
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
