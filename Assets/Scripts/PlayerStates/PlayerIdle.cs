using UnityEngine;

public class PlayerIdle : BasePlayerState
{
    public PlayerIdle(PlayerStateMachine psm) : base(psm) { }

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
        if (!PSM.IsGrounded)
        {
            PSM.ChangeState(PSM.FallState);
            return;
        }

        if (Input.MoveDir != Vector2.zero)
        {
            PSM.ChangeState(PSM.MoveState);
            return;
        }

        if (Input.IsJumpPressed && PSM.IsGrounded)
        {
            PSM.ChangeState(PSM.JumpState);
            return;
        }
    }

    public override void FixedUpdate()
    {
        float decelleration = PSM.IsGrounded ? Stats.GroundDeceleration : Stats.AirDeceleration;
        MoveVelocity = Vector2.Lerp(MoveVelocity, Vector2.zero, decelleration * Time.fixedDeltaTime);
        Rb.velocity = new Vector2(MoveVelocity.x, Rb.velocity.y);
    }


    public override void Exit() { /* Nessuna logica speciale all'uscita */ }
}
