using UnityEngine;

/// <summary>
/// Stato "Move": gestisce camminata e corsa a terra / in aria.
/// </summary>
public class PlayerMove : BasePlayerState
{
    public PlayerMove(PlayerStateMachine psm) : base(psm) { }

    // ───────────────────────────────────────────────────────────────────── STATO
    public override void Enter()
    {
        // Imposta animazione di movimento (se serve)
    }

    public override void Update()
    {
        // 1) Transizione verso Idle se il giocatore smette di muoversi
        if (Input.MoveDir == Vector2.zero)
        {
            PSM.ChangeState(PSM.IdleState);
            return;
        }

        // 2) Transizione verso Jump se parte il salto ed è a terra
        if (Input.IsJumpPressed && PSM.IsGrounded)
        {
            //PSM.ChangeState(PSM.JumpState);
            return;
        }
    }

    public override void FixedUpdate()
    {
        // Calcola parametri a seconda che il player sia a terra o in aria
        float acc = PSM.IsGrounded ? Stats.GroundAcceleration : Stats.AirAcceleration;
        float dec = PSM.IsGrounded ? Stats.GroundDeceleration : Stats.AirDeceleration;

        Move(acc, dec, Input.MoveDir);
    }

    public override void Exit() { /* nessuna logica particolare */ }

    // ──────────────────────────────────────────────────────────────── MOVIMENTO
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            // Velocità di destinazione in base a corsa o camminata
            float maxSpeed = Input.IsRunHeld ? Stats.MaxRunSpeed : Stats.MaxWalkSpeed;
            Vector2 targetVelocity = new Vector2(moveInput.x * maxSpeed, RB.velocity.y);

            // Interpola verso la velocità di destinazione
            MoveVelocity = Vector2.Lerp(MoveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            RB.velocity = new Vector2(MoveVelocity.x, RB.velocity.y);

            // Flip sprite se necessario
            PSM.FlipSprite(moveInput.x);
        }
        else // nessun input → decelerazione
        {
            MoveVelocity = Vector2.Lerp(MoveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            RB.velocity = new Vector2(MoveVelocity.x, RB.velocity.y);
        }
    }
}