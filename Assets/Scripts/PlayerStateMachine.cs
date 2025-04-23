using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SOMovementStats _movementStats;
    public SOMovementStats MovementStats => _movementStats;

    public GameInput Input { get; private set; }
    public Animator Animator { get; private set; }
    public CollisionChecker CollisionChecker { get; private set; }

    [Header("Colliders")]
    [SerializeField] private Collider2D _feetColl;
    public Collider2D FeetColl => _feetColl;
    [SerializeField] private Collider2D _bodyColl;
    public Collider2D BodyColl => _bodyColl;

    public Rigidbody2D Rb { get; private set; }
    public Vector2 MoveVelocity { get; set; }

    public bool IsFacingRight { get; private set; } = true;
    public bool IsGrounded { get; private set; }
    public bool BumpedHead { get; private set; }

    public PlayerIdle IdleState { get; private set; }
    public PlayerMove MoveState { get; private set; }
    public PlayerJump JumpState { get; private set; }
    public PlayerFall FallState { get; private set; }

    private IPlayerState _currentState;

    #region Unity lifecycle
    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Input = GameInput.Instance;
        Animator = GetComponent<Animator>();

        // Initialize collision checker
        CollisionChecker = new CollisionChecker(this);

        IdleState ??= new PlayerIdle(this);
        MoveState ??= new PlayerMove(this);
        JumpState ??= new PlayerJump(this);
        FallState ??= new PlayerFall(this);

        ChangeState(IdleState);
    }

    private void Update()
    {
        _currentState?.Update();
    }

    private void FixedUpdate()
    {
        // Update collision flags
        IsGrounded = CollisionChecker.IsGrounded();
        BumpedHead = CollisionChecker.BumpedHead();

        _currentState?.FixedUpdate();
    }
    #endregion

    #region State management helpers
    public void ChangeState(IPlayerState newState)
    {
        if (newState == _currentState) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
    #endregion

    public void FlipSprite(float xDirection)
    {
        if ((xDirection > 0 && !IsFacingRight) || (xDirection < 0 && IsFacingRight))
        {
            transform.Rotate(0f, 180f, 0f);
            IsFacingRight = !IsFacingRight;
        }
    }
}
