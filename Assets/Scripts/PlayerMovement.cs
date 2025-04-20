using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public SOMovementStats _moveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;
    private Rigidbody2D _rb;
    //movement vars
    private Vector2 _moveVelocity;
    private bool _isFacingRight;
    //collision check vars
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    // jump vars
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    // apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    // jump buffer vars
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    // coyote time vars
    private float _coyoteTimer;



    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _isFacingRight = true;
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        BumpedHead();
        CollisionChecks();
        if (_isGrounded)
        {
            Move(_moveStats.GroundAcceleration, _moveStats.GroundDeceleration, GameInput.Instance.MoveDir);
        }
        else
        {
            Move(_moveStats.AirAcceleration, _moveStats.AirDeceleration, GameInput.Instance.MoveDir);
        }
        Jump();
    }


    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            FlipSprite(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            //if (GameInput.Instance.IsRunHeld)
            //{
            //    targetVelocity = new Vector2(moveInput.x, 0f) * _moveStats.MaxRunSpeed;
            //}
            //else { targetVelocity = new Vector2(moveInput.x, 0f) * _moveStats.MaxWalkSpeed; }

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }
        else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        }
    }

    #endregion

    #region Jump
    private void JumpChecks()
    {
        if (GameInput.Instance.IsJumpPressed)
        {
            _jumpBufferTimer = _moveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }
        if (GameInput.Instance.IsJumpReleased)
        {
            if (_jumpBufferTimer >0f)
            {
                _jumpReleasedDuringBuffer = true;
            }
            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = _moveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //Initiate Jump
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);
            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        //DoubleJump
        else if (_jumpBufferTimer > 0f && _isGrounded && _numberOfJumpsUsed < _moveStats.NumberOfjumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        //AirJump After Coyote TimeLapsed
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < _moveStats.NumberOfjumpsAllowed -1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        //Landed
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }
        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpUsed;
        VerticalVelocity = _moveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        //Apply gravity while jumping
        if (_isJumping)
        {

            //Check for Head Bump
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }
            //gravity on ashending
            if (VerticalVelocity >= 0f)
            {
                //Apex controls
                _apexPoint = Mathf.InverseLerp(_moveStats.InitialJumpVelocity, 0f, VerticalVelocity);
                if (_apexPoint > _moveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _timePastApexThreshold = 0f;
                        _isPastApexThreshold = true;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < _moveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                // Gravity on ascneding but not past apex threshold
                else
                {
                    VerticalVelocity += _moveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            //gravity on descending
            else if (!_isFastFalling)
            {
                VerticalVelocity += _moveStats.Gravity * _moveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }
        //jumpcut
        if (_isFastFalling)
        {
            if (_fastFallTime >= _moveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += _moveStats.Gravity * _moveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < _moveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / _moveStats.TimeForUpwardsCancel));
            }
            _fastFallTime += Time.fixedDeltaTime;
        }
        //normal gravity while falling
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }
            VerticalVelocity += _moveStats.Gravity * Time.fixedDeltaTime;
        }
        //clamp fall speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -_moveStats.MaxFallSpeed, 50f);
        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
    }
    
    #endregion


    #region Timers
    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;
        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        } else { _coyoteTimer = _moveStats.JumpCoyoteTime; }
    }
    #endregion
    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new (_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new (_feetColl.bounds.size.x * _moveStats.HeadWidth, _moveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, _moveStats.HeadDetectionRayLength, _moveStats.GroundLayer);
        if (_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else { _bumpedHead = false; }

        #region Debug Visualization

        if (_moveStats.DebugShowHeadBumpBox)
        {
            float headWidth = _moveStats.HeadWidth;

            Color rayColor;
            if (_bumpedHead)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * _moveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastSize.x / 2 * headWidth + boxCastOrigin.x, boxCastOrigin.y), Vector2.up * _moveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + _moveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }

        #endregion
    }

    #region CollisionChecks
    private void CollisionChecks()
    {
        IsGrounded();
    }

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, _moveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, _moveStats.GroundDetectionRayLength, _moveStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else { _isGrounded = false; }

        #region Debug Visualization
        if (_moveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if (_isGrounded)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * _moveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * _moveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - _moveStats.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }
    #endregion

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;

        Vector2 velocity = new Vector2(moveSpeed, _moveStats.InitialJumpVelocity);
        Gizmos.color = gizmoColor;

        float timeStep = 2 * _moveStats.TimeTillJumpApex / _moveStats.ArcResolution;

        for (int i = 0; i < _moveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < _moveStats.TimeTillJumpApex)
            {
                // Ascending
                displacement = velocity * simulationTime
                             + 0.5f * new Vector2(0, _moveStats.Gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < _moveStats.TimeTillJumpApex + _moveStats.ApexHangTime)
            {
                // Hang phase
                float apexTime = simulationTime - _moveStats.TimeTillJumpApex;
                displacement = velocity * _moveStats.TimeTillJumpApex
                             + 0.5f * new Vector2(0, _moveStats.Gravity) * _moveStats.TimeTillJumpApex * _moveStats.TimeTillJumpApex;
                // ← here: use moveSpeed, not speed
                displacement += new Vector2(moveSpeed, 0) * apexTime;
            }
            else
            {
                // Descending
                float descendTime = simulationTime - _moveStats.TimeTillJumpApex - _moveStats.ApexHangTime;
                displacement = velocity * _moveStats.TimeTillJumpApex
                             + 0.5f * new Vector2(0, _moveStats.Gravity) * _moveStats.TimeTillJumpApex * _moveStats.TimeTillJumpApex;
                displacement += new Vector2(moveSpeed, 0) * _moveStats.ApexHangTime;
                displacement += new Vector2(moveSpeed, 0) * descendTime
                             + 0.5f * new Vector2(0, _moveStats.Gravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (_moveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition,
                    drawPoint - previousPosition,
                    Vector2.Distance(previousPosition, drawPoint),
                    _moveStats.GroundLayer);

                if (hit.collider != null)
                {
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

    private void FlipSprite(Vector2 moveInput)
    {
        if ((moveInput.x > 0 && !_isFacingRight) || (moveInput.x < 0 && _isFacingRight))
        {
            transform.Rotate(0f, 180f, 0f);
            _isFacingRight = !_isFacingRight;
        }
    }

    private void OnDrawGizmos()
    {
        if (_moveStats.ShowWalkJumpArc)
        {
            DrawJumpArc(_moveStats.MaxWalkSpeed, Color.white);
        }
        if (_moveStats.ShowRunJumpArc)
        {
            DrawJumpArc(_moveStats.MaxRunSpeed, Color.red);
        }
    }

}