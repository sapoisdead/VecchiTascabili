using UnityEngine;

public class PlayerMove : BasePlayerState
{
    public PlayerMove(CharacterController pc) : base(pc) { }

   
    public override void Enter()
    {

    }

    public override void Update()
    {
        if (!PC.IsGrounded)
        {
            PC.ChangeState(PC.FallState);
            return;
        }

        if (Input.MoveDir == Vector2.zero)
        {
            PC.ChangeState(PC.IdleState);
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
        // Calcola parametri a seconda che il player sia a terra o in aria
        float acc = PC.IsGrounded ? Stats.GroundAcceleration : Stats.AirAcceleration;
        float dec = PC.IsGrounded ? Stats.GroundDeceleration : Stats.AirDeceleration;

        Move(acc, dec, Input.MoveDir);
    }

    public override void Exit() { /* nessuna logica particolare */ }

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            // Velocità di destinazione in base a corsa o camminata
            float maxSpeed = Input.IsRunHeld ? Stats.MaxRunSpeed : Stats.MaxWalkSpeed;
            Vector2 targetVelocity = new Vector2(moveInput.x * maxSpeed, Rb.velocity.y);

            // Interpola verso la velocità di destinazione
            MoveVelocity = Vector2.Lerp(MoveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            Rb.velocity = new Vector2(MoveVelocity.x, Rb.velocity.y);

            // Flip sprite se necessario
            PC.FlipSprite(moveInput.x);
        }
        else // nessun input → decelerazione
        {
            MoveVelocity = Vector2.Lerp(MoveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            Rb.velocity = new Vector2(MoveVelocity.x, Rb.velocity.y);
        }
    }
}