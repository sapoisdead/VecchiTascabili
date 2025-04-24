using UnityEngine;

public class PlayerJump : BasePlayerState
{
    private enum JumpPhase { Rising, ApexHang, Falling, FastFalling }

    private JumpPhase _phase;
    private float _verticalVelocity;
    private int _jumpsUsed;
    private readonly GraceTimer _coyoteTimer = new();
    private readonly GraceTimer _jumpBuffer = new();
    private float _apexTimer;
    private float _fastFallTimer;
    private float _fastFallStartSpeed;
    private bool _releasedDuringBuffer;

    public PlayerJump(CharacterController pc) : base(pc) { }

    public override void Enter()
    {
        // Capture any jump‐release during buffer
        _releasedDuringBuffer = Input.IsJumpReleased;

        _coyoteTimer.Start(Stats.JumpCoyoteTime);
        _jumpBuffer.Reset();
        _jumpsUsed = 1;

        _phase = JumpPhase.Rising;
        _verticalVelocity = Stats.InitialJumpVelocity;
        Rb.velocity = new Vector2(Rb.velocity.x, _verticalVelocity);

        // If released during buffer, cancel upward immediately
        if (_releasedDuringBuffer)
            BeginFastFall();
    }

    public override void Update()
    {
        // Tick timers
        _coyoteTimer.Tick(Time.deltaTime);
        if (Input.IsJumpPressed)
            _jumpBuffer.Start(Stats.JumpBufferTime);
        else
            _jumpBuffer.Tick(Time.deltaTime);

        // Early release at any rise or apex hang → fast fall
        if (Input.IsJumpReleased &&
            (_phase == JumpPhase.Rising || _phase == JumpPhase.ApexHang))
        {
            BeginFastFall();
            return;
        }

        // Landing exit after ascending
        if (_phase != JumpPhase.Rising && PC.CollisionChecker.IsGrounded())
        {
            PC.ChangeState(PC.IdleState);
            return;
        }

        // Buffered air jump
        if (_jumpBuffer.Active && _jumpsUsed < Stats.NumberOfjumpsAllowed)
        {
            StartAirJump();
            return;
        }
    }

    public override void FixedUpdate()
    {
        // Head bump breaks rising
        if (_phase == JumpPhase.Rising && PC.CollisionChecker.BumpedHead())
            BeginFastFall();

        // Phase‐based physics
        switch (_phase)
        {
            case JumpPhase.Rising: HandleRising(); break;
            case JumpPhase.ApexHang: HandleApexHang(); break;
            case JumpPhase.Falling: HandleFalling(); break;
            case JumpPhase.FastFalling: HandleFastFall(); break;
        }

        // Clamp and apply velocity
        _verticalVelocity = Mathf.Clamp(_verticalVelocity, -Stats.MaxFallSpeed, float.MaxValue);
        Rb.velocity = new Vector2(Rb.velocity.x, _verticalVelocity);
    }

    public override void Exit()
    {
        // Clear buffer flag
        _releasedDuringBuffer = false;
    }

    private void StartAirJump()
    {
        _jumpsUsed++;
        _jumpBuffer.Reset();
        _phase = JumpPhase.Rising;
        _verticalVelocity = Stats.InitialJumpVelocity;
        Rb.velocity = new Vector2(Rb.velocity.x, _verticalVelocity);
        //PSM.Animator?.Play("Jump");
    }

    private void BeginFastFall()
    {
        _phase = JumpPhase.FastFalling;
        _fastFallTimer = 0f;
        _fastFallStartSpeed = _verticalVelocity;
    }

    private void HandleRising()
    {
        float apexPoint = Mathf.InverseLerp(Stats.InitialJumpVelocity, 0f, _verticalVelocity);
        if (apexPoint > Stats.ApexThreshold)
        {
            _phase = JumpPhase.ApexHang;
            _apexTimer = 0f;
            return;
        }

        // Normal ascending gravity
        _verticalVelocity += Stats.Gravity * Time.fixedDeltaTime;
    }

    private void HandleApexHang()
    {
        _apexTimer += Time.fixedDeltaTime;
        if (_apexTimer < Stats.ApexHangTime)
        {
            _verticalVelocity = 0f; // hang in place
        }
        else
        {
            // transition to falling
            _phase = JumpPhase.Falling;
            _verticalVelocity = -0.01f;
        }
    }

    private void HandleFalling()
    {
        // normal gravity
        _verticalVelocity += Stats.Gravity * Time.fixedDeltaTime;
    }

    private void HandleFastFall()
    {
        _fastFallTimer += Time.fixedDeltaTime;
        if (_fastFallTimer < Stats.TimeForUpwardsCancel)
        {
            _verticalVelocity = Mathf.Lerp(_fastFallStartSpeed, 0f, _fastFallTimer / Stats.TimeForUpwardsCancel);
        }
        else
        {
            _verticalVelocity += Stats.Gravity * Stats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
        }
    }
}
