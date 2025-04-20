using UnityEngine;

public class PlayerFall : BasePlayerState
{
    private readonly GraceTimer _coyoteTimer = new();
    private readonly GraceTimer _jumpBuffer = new();

    public PlayerFall(PlayerStateMachine psm) : base(psm) { }

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
        if (_jumpBuffer.Active && PSM.CollisionChecker.IsGrounded())
        {
            PSM.ChangeState(PSM.JumpState);
            return;
        }

        // 3) if pressed *during* the coyote window, jump too
        if (Input.IsJumpPressed && _coyoteTimer.Active)
        {
            PSM.ChangeState(PSM.JumpState);
            return;
        }

        // 4) landing without jump: go to Move or Idle
        if (PSM.CollisionChecker.IsGrounded())
        {
            if (Input.MoveDir != Vector2.zero)
                PSM.ChangeState(PSM.MoveState);
            else
                PSM.ChangeState(PSM.IdleState);
        }
    }

    public override void FixedUpdate()
    {
        // Apply gravity
        float newY = RB.velocity.y + Stats.Gravity * Time.fixedDeltaTime;
        newY = Mathf.Clamp(newY, -Stats.MaxFallSpeed, float.MaxValue);
        RB.velocity = new Vector2(RB.velocity.x, newY);
    }

    public override void Exit() { }
}
