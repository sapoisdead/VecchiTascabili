using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementStats _movementStats;
    public PlayerMovementStats MovementStats
    {
        get { return _movementStats; }
        private set { _movementStats = value; }
    }
    public GameInput Input { get; private set; }
    public Animator Animator { get; private set; }

    [Header("Colliders")]
    [SerializeField] private Collider2D feetColl;
    [SerializeField] private Collider2D bodyColl;

    public Rigidbody2D RB { get; private set; }
    public Vector2 MoveVelocity { get; set; }

    public bool IsFacingRight { get; private set; } = true;

    public bool IsGrounded { get; private set; }
    public bool BumpedHead { get; private set; }

    public PlayerIdle IdleState { get; private set; }
    public PlayerMove MoveState { get; private set; }
    public PlayerJump JumpState{ get; private set; }

    private IPlayerState _currentState;


    // ════════════════════════════════════════════════════════════════════════════
    #region Unity lifecycle
    // ════════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Input = GameInput.Instance;

        IdleState ??= new PlayerIdle(this);
        MoveState ??= new PlayerMove(this);
        //JumpState ??= new PlayerJump(this);

        ChangeState(IdleState);
    }

    private void Update() => _currentState?.Update();

    private void FixedUpdate()
    {
        CollisionChecks();
        _currentState?.FixedUpdate();
    }
    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    #region State management helpers
    // ════════════════════════════════════════════════════════════════════════════
    public void ChangeState(IPlayerState newState)
    {
        if (newState == _currentState) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    #region Collision checks
    // ════════════════════════════════════════════════════════════════════════════
    private void CollisionChecks()
    {
        IsGrounded = CheckGrounded();
        BumpedHead = CheckBumpedHead();
    }

    private bool CheckGrounded()
    {
        Vector2 origin = new (feetColl.bounds.center.x, feetColl.bounds.min.y);
        Vector2 size = new (feetColl.bounds.size.x, MovementStats.GroundDetectionRayLength);

        var hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, MovementStats.GroundDetectionRayLength, MovementStats.GroundLayer);
        return hit.collider != null;
    }

    private bool CheckBumpedHead()
    {
        if (!MovementStats.HeadDetectionRayLength.Equals(0f))
        {
            Vector2 origin = new (bodyColl.bounds.center.x, bodyColl.bounds.max.y);
            Vector2 size = new (bodyColl.bounds.size.x * MovementStats.HeadWidth, MovementStats.HeadDetectionRayLength);

            var hit = Physics2D.BoxCast(origin, size, 0f, Vector2.up, MovementStats.HeadDetectionRayLength, MovementStats.GroundLayer);
            return hit.collider != null;
        }
        return false;
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────────────────
    //  Utility
    // ─────────────────────────────────────────────────────────────────────────────
    public void FlipSprite(float xDirection)
    {
        if ((xDirection > 0 && !IsFacingRight) || (xDirection < 0 && IsFacingRight))
        {
            transform.Rotate(0f, 180f, 0f);
            IsFacingRight = !IsFacingRight;
        }
    }
}
