using UnityEngine;

public class PlayerJump : BasePlayerState
{
    // Jump state variables
    private bool _isJumping;
    private bool _isFalling;
    private bool _isFastFalling;
    private bool _isPastApex;

    private float _verticalVelocity;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private float _apexPoint;
    private float _timePastApex;
    private int _jumpsUsed;

    public PlayerJump(PlayerStateMachine psm) : base(psm) { }

    public override void Enter()
    {
        _isJumping = true;
        _isFalling = false;
        _isFastFalling = false;
        _isPastApex = false;
        _fastFallTime = 0f;
        _timePastApex = 0f;

        _jumpsUsed++;
        _verticalVelocity = Stats.InitialJumpVelocity;
    }

    public override void Update()
    {
        // Handle buffered jump presses for mid-air jumps
        if (Input.IsJumpPressed && _jumpsUsed < Stats.NumberOfjumpsAllowed)
        {
            _isFastFalling = false;
            _fastFallTime = 0f;

            _jumpsUsed++;
            _verticalVelocity = Stats.InitialJumpVelocity;
            _isJumping = true;
            _isFalling = false;
        }

        // Early jump cut on release
        if (Input.IsJumpReleased && _isJumping && _verticalVelocity > 0f)
        {
            _isFastFalling = true;
            _fastFallReleaseSpeed = _verticalVelocity;
        }
    }

    public override void FixedUpdate()
    {
        if (PSM.BumpedHead && _verticalVelocity > 0f)
        {
            _isFastFalling = true;
        }

        // Rising phase
        if (_isJumping)
        {
            if (_verticalVelocity > 0f)
            {
                _apexPoint = Mathf.InverseLerp(Stats.InitialJumpVelocity, 0f, _verticalVelocity);
                if (_apexPoint > Stats.ApexThreshold)
                {
                    if (!_isPastApex)
                    {
                        _isPastApex = true;
                        _timePastApex = 0f;
                    }

                    _timePastApex += Time.fixedDeltaTime;
                    if (_timePastApex < Stats.ApexHangTime)
                        _verticalVelocity = 0f;
                    else
                        _verticalVelocity = -0.01f;
                }
                else
                {
                    _verticalVelocity += Stats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApex)
                        _isPastApex = false;
                }
            }
            else
            {
                // Start falling
                _isJumping = false;
                _isFalling = true;
            }
        }

        // Fast-fall
        if (_isFastFalling)
        {
            if (_fastFallTime < Stats.TimeForUpwardsCancel)
                _verticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, _fastFallTime / Stats.TimeForUpwardsCancel);
            else
                _verticalVelocity += Stats.Gravity * Stats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            _fastFallTime += Time.fixedDeltaTime;
        }
        // Normal fall
        else if (_isFalling)
        {
            _verticalVelocity += Stats.Gravity * Time.fixedDeltaTime;
        }
        // Walked off a ledge
        else if (!PSM.IsGrounded)
        {
            _isFalling = true;
            _verticalVelocity += Stats.Gravity * Time.fixedDeltaTime;
        }

        _verticalVelocity = Mathf.Clamp(_verticalVelocity, -Stats.MaxFallSpeed, float.MaxValue);

        float targetX = Input.MoveDir.x * (Input.IsRunHeld ? Stats.MaxRunSpeed : Stats.MaxWalkSpeed);
        PSM.MoveVelocity = Vector2.MoveTowards(
            PSM.MoveVelocity,
            new Vector2(targetX, 0f),
            (Mathf.Abs(Input.MoveDir.x) > 0.01f ? Stats.AirAcceleration : Stats.AirDeceleration) * Time.fixedDeltaTime
        );

        PSM.RB.velocity = new Vector2(PSM.MoveVelocity.x, _verticalVelocity);

        if (PSM.IsGrounded && _verticalVelocity <= 0f)
        {
            PSM.ChangeState(Input.MoveDir.x != 0f ? PSM.MoveState : PSM.IdleState);
        }
    }

    public override void Exit()
    {
        _isJumping = false;
        _isFalling = false;
        _isFastFalling = false;
        _isPastApex = false;

        _jumpsUsed = 0; // Reset jump count on exit
    }
}


