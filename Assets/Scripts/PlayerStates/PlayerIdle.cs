using UnityEngine;

public class PlayerIdle : BasePlayerState
{
    public PlayerIdle(PlayerStateMachine psm) : base(psm) { }

    public override void Enter()
    {
        // Azzeri la velocità orizzontale e imposti l'animazione idle
        MoveVelocity = Vector2.zero;
        RB.velocity = new Vector2(0f, RB.velocity.y);
        //PSM.Animator?.Play("Idle");
    }

    public override void Update()
    {
        if (!PSM.IsGrounded)
        {
            PSM.ChangeState(PSM.JumpState);
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
        // Applica un leggero deceleration per evitare scivolamenti residui
        float decelleration = PSM.IsGrounded ? Stats.GroundDeceleration : Stats.AirDeceleration;
        MoveVelocity = Vector2.Lerp(MoveVelocity, Vector2.zero, decelleration * Time.fixedDeltaTime);
        RB.velocity = new Vector2(MoveVelocity.x, RB.velocity.y);
    }

    public override void Exit() { /* Nessuna logica speciale all'uscita */ }
}
