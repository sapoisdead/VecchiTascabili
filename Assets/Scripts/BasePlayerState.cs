using UnityEngine;

public abstract class BasePlayerState : IPlayerState
{
    protected readonly PlayerStateMachine PSM;

    //  Accessors “live” invece di copie
    protected GameInput Input => PSM.Input;
    protected SOMovementStats Stats => PSM.MovementStats;
    protected Rigidbody2D Rb => PSM.Rb;

    protected Vector2 MoveVelocity
    {
        get => PSM.MoveVelocity;
        set => PSM.MoveVelocity = value;
    }

    protected BasePlayerState(PlayerStateMachine psm)
    {
        PSM = psm;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Exit();
}
