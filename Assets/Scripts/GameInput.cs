using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    private PlayerInput _input;

    public event EventHandler<MoveArgs> OnMove; 
    public event EventHandler OnRunPressed, OnRunReleased;
    public event EventHandler OnJumpPressed, OnJumpReleased;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one GAMEINPUT in the current scene");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _input = new PlayerInput();
    }

    private void OnEnable()
    {
        _input.Enable();

        _input.Player.Move.performed += OnMove_performed;
        _input.Player.Move.canceled += OnMove_canceled;
        _input.Player.Run.started += OnRun_started;
        _input.Player.Run.canceled += OnRun_canceled;
        _input.Player.Jump.started += OnJump_started;
        _input.Player.Jump.canceled += OnJump_canceled;
    }

    private void OnMove_performed(InputAction.CallbackContext obj)
    {
        OnMove?.Invoke(this, new MoveArgs(obj.ReadValue<Vector2>().normalized));
    }
    private void OnMove_canceled(InputAction.CallbackContext obj)
    {
        OnMove?.Invoke(this, new MoveArgs(Vector2.zero));
    }
    private void OnRun_started(InputAction.CallbackContext obj)
    {
        OnRunPressed?.Invoke(this, EventArgs.Empty);
    }
    private void OnRun_canceled(InputAction.CallbackContext obj)
    {
        OnRunReleased?.Invoke(this, EventArgs.Empty);
    }
    private void OnJump_started(InputAction.CallbackContext obj)
    {
        OnJumpPressed?.Invoke(this, EventArgs.Empty);
    }
    private void OnJump_canceled(InputAction.CallbackContext obj)
    {
        OnJumpReleased?.Invoke(this, EventArgs.Empty);
    }
}

public sealed class MoveArgs : EventArgs
{
    public Vector2 MoveDir { get; }

    public MoveArgs(Vector2 moveDir)
    {
        MoveDir = moveDir; 
    }
}
public sealed class JumpArgs : EventArgs
{
    public bool IsJumping { get; }
    public uint Stamina { get; }

    public JumpArgs(bool isJumping, uint stamina)
    {
        IsJumping = isJumping;
        Stamina = stamina;
    }
}
