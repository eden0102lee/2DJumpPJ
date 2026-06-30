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

        public int FacingDirection { get; private set; } = 1;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _wallJumpLockTimer;
        private float _dashCooldownTimer;
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

            Rigidbody.gravityScale = config.gravityScale;
            Rigidbody.freezeRotation = true;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

            StateMachine = new HeroStateMachine();
            StateMachine.RegisterState(new HeroIdleState(this));
            StateMachine.RegisterState(new HeroRunState(this));
            StateMachine.RegisterState(new HeroJumpState(this));
            StateMachine.RegisterState(new HeroFallState(this));
            StateMachine.RegisterState(new HeroWallSlideState(this));
            StateMachine.RegisterState(new HeroDashState(this));
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
                StateMachine.ChangeState(typeof(HeroJumpState));
                return true;
            }

            if (Sensor.IsTouchingWall && !Sensor.IsGrounded)
            {
                PerformWallJump();
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

        public void PerformWallJump()
        {
            FacingDirection = -Sensor.WallDirection;
            _wallJumpLockTimer = config.wallJumpLockTime;
            Rigidbody.linearVelocity = new Vector2(
                config.wallJumpForce.x * FacingDirection,
                config.wallJumpForce.y);
        }

        public bool ShouldWallSlide()
        {
            if (Sensor.IsGrounded || !Sensor.IsTouchingWall)
                return false;

            var pressingTowardWall = Mathf.Sign(Input.MoveInput.x) == Sensor.WallDirection;
            return pressingTowardWall || RbIsFalling();
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
    }
}
