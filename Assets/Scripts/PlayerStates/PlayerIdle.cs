using UnityEngine;

public class PlayerIdle : BasePlayerState
{
    public PlayerIdle(CharacterController pc) : base(pc) { }

    public override void Enter()
    {
        // clear horizontal
        MoveVelocity = Vector2.zero;
        // clear any leftover vertical velocity
        Rb.velocity = new Vector2(0f, Physics2D.gravity.y);

        // PSM.Animator?.Play("Idle");
    }

    public override void Update()
    {
        if (!PC.IsGrounded)
        {
            PC.ChangeState(PC.FallState);
            return;
        }

        if (Input.MoveDir != Vector2.zero)
        {
            PC.ChangeState(PC.MoveState);
            return;
        }

        if (Input.IsJumpPressed && PC.IsGrounded)
        {
            PC.ChangeState(PC.JumpState);
            return;
        }
    }

    public override void FixedUpdate()
    {
        float decelleration = PC.IsGrounded ? Stats.GroundDeceleration : Stats.AirDeceleration;
        MoveVelocity = Vector2.Lerp(MoveVelocity, Vector2.zero, decelleration * Time.fixedDeltaTime);
        Rb.velocity = new Vector2(MoveVelocity.x, Rb.velocity.y);
    }


    public override void Exit() { /* Nessuna logica speciale all'uscita */ }
}
