using Hollow.Core;
using Hollow.Input;
using UnityEngine;

namespace Hollow.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(GroundWallSensor))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class HeroController : MonoBehaviour
    {
        [SerializeField] private HeroControllerConfig config;

        public HeroControllerConfig Config => config;
        public Rigidbody2D Rigidbody { get; private set; }
        public GroundWallSensor Sensor { get; private set; }
        public PlayerInputReader Input { get; private set; }
        public HeroStateMachine StateMachine { get; private set; }
        public HeroAnimator HeroAnimator { get; private set; }

        public bool CanMove { get; set; } = true;
        public bool CanDash { get; set; } = true;
        public bool IsInvulnerable { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool CameFromSprint { get; private set; }

        public int FacingDirection { get; private set; } = 1;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _wallJumpLockTimer;
        private float _dashCooldownTimer;
        private float _iFrameTimer;
        private bool _wasGrounded;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("HeroController requires a HeroControllerConfig asset.", this);
                enabled = false;
                return;
            }

            Rigidbody = GetComponent<Rigidbody2D>();
            Sensor = GetComponent<GroundWallSensor>();
            Input = GetComponent<PlayerInputReader>();
            HeroAnimator = GetComponent<HeroAnimator>();

            Input.SetDashTapThreshold(config.dashTapThreshold);

            Rigidbody.gravityScale = config.gravityScale;
            Rigidbody.freezeRotation = true;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

            StateMachine = new HeroStateMachine();
            StateMachine.RegisterState(new HeroIdleState(this));
            StateMachine.RegisterState(new HeroRunState(this));
            StateMachine.RegisterState(new HeroSprintState(this));
            StateMachine.RegisterState(new HeroJumpState(this));
            StateMachine.RegisterState(new HeroFallState(this));
            StateMachine.RegisterState(new HeroWallSlideState(this));
            StateMachine.RegisterState(new HeroWallClimbState(this));
            StateMachine.RegisterState(new HeroDashState(this));
            StateMachine.RegisterState(new HeroDownDashState(this));
        }

        private void Start()
        {
            StateMachine.Initialize(typeof(HeroIdleState));
        }

        private void Update()
        {
            UpdateFacing();
            UpdateTimers();
            Sensor.UpdateSensor(FacingDirection);
            StateMachine.Tick();
            HeroAnimator?.UpdateAnimator(this);
        }

        private void FixedUpdate()
        {
            StateMachine.FixedTick();
            CheckLanding();
        }

        private void UpdateFacing()
        {
            if (_wallJumpLockTimer > 0f)
                return;

            if (Mathf.Abs(Input.MoveInput.x) > 0.01f)
                FacingDirection = Input.MoveInput.x > 0f ? 1 : -1;
        }

        private void UpdateTimers()
        {
            if (Sensor.IsGrounded)
                _coyoteTimer = config.coyoteTime;
            else
                _coyoteTimer -= Time.deltaTime;

            if (Input.JumpPressed)
                _jumpBufferTimer = config.jumpBufferTime;
            else
                _jumpBufferTimer -= Time.deltaTime;

            if (_wallJumpLockTimer > 0f)
                _wallJumpLockTimer -= Time.deltaTime;

            if (_dashCooldownTimer > 0f)
                _dashCooldownTimer -= Time.deltaTime;

            if (_iFrameTimer > 0f)
            {
                _iFrameTimer -= Time.deltaTime;
                IsInvulnerable = _iFrameTimer > 0f;
            }
            else
            {
                IsInvulnerable = false;
            }
        }

        private void CheckLanding()
        {
            if (Sensor.IsGrounded && !_wasGrounded)
                GameEvents.RaisePlayerLanded();

            _wasGrounded = Sensor.IsGrounded;
        }

        public float GetEffectiveMoveInput()
        {
            var input = Input.MoveInput.x;

            if (_wallJumpLockTimer > 0f && Mathf.Sign(input) == FacingDirection)
                return 0f;

            return input;
        }

        public bool TryConsumeJump()
        {
            if (_jumpBufferTimer <= 0f)
                return false;

            if (Sensor.IsGrounded || _coyoteTimer > 0f)
            {
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                CameFromSprint = IsSprinting;
                StateMachine.ChangeState(typeof(HeroJumpState));
                return true;
            }

            var wallDir = Sensor.GetWallDirectionFromInput(Input.MoveInput.x);
            if (wallDir != 0 && !Sensor.IsGrounded)
            {
                PerformWallJump(wallDir);
                _jumpBufferTimer = 0f;
                StateMachine.ChangeState(typeof(HeroFallState));
                return true;
            }

            return false;
        }

        public void PerformJump(float force)
        {
            Rigidbody.linearVelocity = new Vector2(Rigidbody.linearVelocity.x, force);
        }

        public void PerformSprintJump()
        {
            var horizontalBoost = FacingDirection * config.runSpeed * config.sprintJumpHorizontalBoost;
            Rigidbody.linearVelocity = new Vector2(horizontalBoost, config.sprintJumpForce);
        }

        public void PerformWallJump(int wallDirection)
        {
            FacingDirection = -wallDirection;
            _wallJumpLockTimer = config.wallJumpLockTime;
            Rigidbody.linearVelocity = new Vector2(
                config.wallJumpForce.x * FacingDirection,
                config.wallJumpForce.y);
        }

        public bool ShouldWallSlide()
        {
            if (Sensor.IsGrounded)
                return false;

            var wallDir = Sensor.GetWallDirectionFromInput(Input.MoveInput.x);
            if (wallDir == 0)
                return false;

            var pressingTowardWall = Mathf.Abs(Input.MoveInput.x) > 0.01f;
            return pressingTowardWall || RbIsFalling();
        }

        public int GetActiveWallDirection()
        {
            var wallDir = Sensor.GetWallDirectionFromInput(Input.MoveInput.x);
            if (wallDir != 0)
                return wallDir;

            if (Sensor.IsTouchingWall)
                return Sensor.WallDirection;

            return 0;
        }

        private bool RbIsFalling() => Rigidbody.linearVelocity.y < 0f;

        public bool TryStartDash()
        {
            if (!CanDash || _dashCooldownTimer > 0f)
                return false;

            if (!Sensor.IsGrounded && !config.allowAirDash)
                return false;

            GameEvents.RaiseDashStarted();
            return true;
        }

        public void StartDashCooldown()
        {
            _dashCooldownTimer = config.dashCooldown;
        }

        public void StartIFrames()
        {
            _iFrameTimer = config.dashIFrameDuration;
            IsInvulnerable = true;
        }

        public void ApplyVariableJumpCut()
        {
            if (Input.JumpReleased && Rigidbody.linearVelocity.y > 0f)
            {
                Rigidbody.linearVelocity = new Vector2(
                    Rigidbody.linearVelocity.x,
                    Rigidbody.linearVelocity.y * config.jumpCutMultiplier);
            }
        }

        public void SetFacing(int direction)
        {
            FacingDirection = direction;
        }

        public void SetSprinting(bool sprinting)
        {
            IsSprinting = sprinting;
        }

        public bool ShouldStartSprint()
        {
            return Input.DashHoldExceeded
                && Sensor.IsGrounded
                && Mathf.Abs(Input.MoveInput.x) > 0.01f;
        }

        public bool ShouldTryGroundDash()
        {
            return Input.DashTap;
        }

        public bool ShouldTryAirDash()
        {
            return Input.DashPressed || Input.DashTap;
        }

        public bool ShouldTryDownDash()
        {
            return config.allowDownDash
                && !Sensor.IsGrounded
                && Input.WantsDownDash
                && (Input.DashPressed || Input.DashTap);
        }

        public bool ShouldTryWallClimb()
        {
            var wallDir = GetActiveWallDirection();
            if (wallDir == 0)
                return false;

            return Input.DashHeld
                && Mathf.Sign(Input.MoveInput.x) == wallDir;
        }

        public bool ShouldTryWallUpDash()
        {
            var wallDir = GetActiveWallDirection();
            if (wallDir == 0)
                return false;

            return Input.DashTap;
        }

        public void PerformWallUpDash()
        {
            Rigidbody.linearVelocity = new Vector2(
                Rigidbody.linearVelocity.x,
                config.wallUpDashForce.y);
            StartIFrames();
        }
    }
}
