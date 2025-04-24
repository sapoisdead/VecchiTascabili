using UnityEngine;

public class PlayerFall : BasePlayerState
{
    private readonly GraceTimer _coyoteTimer = new();
    private readonly GraceTimer _jumpBuffer = new();

    public PlayerFall(CharacterController pc) : base(pc) { }

    public override void Enter()
    {
        // Start coyote time when we begin falling
        _coyoteTimer.Start(Stats.JumpCoyoteTime);

        // Play fall animation if any
        //PSM.Animator?.Play("Fall");
    }

    public override void Update()
    {
        // 1) tick both timers
        _coyoteTimer.Tick(Time.deltaTime);
        if (Input.IsJumpPressed) _jumpBuffer.Start(Stats.JumpBufferTime);
        else _jumpBuffer.Tick(Time.deltaTime);

        // 2) if buffered *and* landed, jump immediately
        if (_jumpBuffer.Active && PC.CollisionChecker.IsGrounded())
        {
            PC.ChangeState(PC.JumpState);
            return;
        }

        // 3) if pressed *during* the coyote window, jump too
        if (Input.IsJumpPressed && _coyoteTimer.Active)
        {
            PC.ChangeState(PC.JumpState);
            return;
        }

        // 4) landing without jump: go to Move or Idle
        if (PC.CollisionChecker.IsGrounded())
        {
            if (Input.MoveDir != Vector2.zero)
                PC.ChangeState(PC.MoveState);
            else
                PC.ChangeState(PC.IdleState);
        }
    }

    public override void FixedUpdate()
    {
        // Apply gravity
        float newY = Rb.velocity.y + Stats.Gravity * Time.fixedDeltaTime;
        newY = Mathf.Clamp(newY, -Stats.MaxFallSpeed, float.MaxValue);
        Rb.velocity = new Vector2(Rb.velocity.x, newY);
    }

    public override void Exit() { }
}
