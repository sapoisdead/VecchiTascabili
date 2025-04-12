using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovStats _moveStats;
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

    private void Start()
    {
        GameInput.Instance.OnJump += Handle_OnJump;
    }

    private void OnDisable()
    {
        GameInput.Instance.OnJump -= Handle_OnJump;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _isFacingRight = true;
    }

    private void FixedUpdate()
    {
        if (_isGrounded)
        {
            Move(_moveStats.GroundAcceleration, _moveStats.GroundDeceleration, GameInput.Instance.MoveDir);
        }
        CollisionChecks();
        if (_isGrounded)
        {
            Move(_moveStats.GroundAcceleration, _moveStats.GroundDeceleration, GameInput.Instance.MoveDir);
        }
        else
        {
            Move(_moveStats.AirAcceleration, _moveStats.AirDeceleration, GameInput.Instance.MoveDir);
        }
    }

    private void Handle_OnJump(object sender, System.EventArgs e)
    {
        _rb.AddForce(Vector2.up * 20f, ForceMode2D.Impulse);
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            FlipSprite(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (GameInput.Instance.IsRunHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * _moveStats.MaxRunSpeed;
            }
            else { targetVelocity = new Vector2(moveInput.x, 0f) * _moveStats.MaxWalkSpeed; }

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

    private void FlipSprite(Vector2 moveInput)
    {
        if ((moveInput.x > 0 && !_isFacingRight) || (moveInput.x < 0 && _isFacingRight))
        {
            transform.Rotate(0f, 180f, 0f);
            _isFacingRight = !_isFacingRight;
        }
    }

}