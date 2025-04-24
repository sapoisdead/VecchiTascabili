using UnityEngine;

public abstract class BasePlayerState : IPlayerState
{
    protected readonly CharacterController PC;

    //  Accessors “live” invece di copie
    protected IInputProvider Input => PC.Input;
    protected SOMovementStats Stats => PC.MovementStats;
    protected Rigidbody2D Rb => PC.Rb;

    protected Vector2 MoveVelocity
    {
        get => PC.MoveVelocity;
        set => PC.MoveVelocity = value;
    }

    protected BasePlayerState(CharacterController pc)
    {
        PC = pc;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();
}
