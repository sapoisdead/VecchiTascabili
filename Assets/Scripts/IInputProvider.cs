using UnityEngine;

public interface IInputProvider
{
    Vector2 MoveDir { get; }
    bool IsRunHeld { get; }
    bool IsJumpPressed { get; }
    bool IsJumpReleased { get; }
}
