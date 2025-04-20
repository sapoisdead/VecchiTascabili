using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInput _input;

    // ─────────────────────────────────────────────────────────────────────────────
    //  Buffered properties (da leggere in qualsiasi Update/FixedUpdate)
    // ─────────────────────────────────────────────────────────────────────────────
    public Vector2 MoveDir { get; private set; }      
    public bool IsRunHeld { get; private set; }   
    public bool IsJumpPressed { get; private set; }   
    public bool IsJumpReleased { get; private set; }
    // ─────────────────────────────────────────────────────────────────────────────
    //  Eventi opzionali (se vuoi logica completamente event‑driven)
    // ─────────────────────────────────────────────────────────────────────────────
    public event EventHandler<MoveArgs> OnMove;
    public event EventHandler OnRunPressed, OnRunReleased;
    public event EventHandler OnJumpPressed, OnJumpReleased;

    // ════════════════════════════════════════════════════════════════════════════
    #region Unity lifecycle
    // ════════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        // Singleton guard
        if (Instance != null)
        {
            Debug.LogError("There's more than one GameInput in the scene!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _input = new PlayerInput();
    }

    private void OnEnable()
    {
        _input.Enable();

        // Mapping callbacks → buffer + eventi
        _input.Player.Move.performed += OnMove_performed;
        _input.Player.Move.canceled += OnMove_canceled;

        _input.Player.Run.started += OnRun_started;
        _input.Player.Run.canceled += OnRun_canceled;

        _input.Player.Jump.started += OnJump_started;
        _input.Player.Jump.canceled += OnJump_canceled;
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    private void LateUpdate()
    {
        // Reset one‑frame flags
        IsJumpPressed = false;
        IsJumpReleased = false;
    }
    #endregion

    // ════════════════════════════════════════════════════════════════════════════
    #region Callbacks
    // ════════════════════════════════════════════════════════════════════════════
    private void OnMove_performed(InputAction.CallbackContext ctx)
    {
        MoveDir = ctx.ReadValue<Vector2>().normalized;
        OnMove?.Invoke(this, new MoveArgs(MoveDir));
    }

    private void OnMove_canceled(InputAction.CallbackContext ctx)
    {
        MoveDir = Vector2.zero;
        OnMove?.Invoke(this, new MoveArgs(MoveDir));
    }

    private void OnRun_started(InputAction.CallbackContext ctx)
    {
        IsRunHeld = true;
        OnRunPressed?.Invoke(this, EventArgs.Empty);
    }

    private void OnRun_canceled(InputAction.CallbackContext ctx)
    {
        IsRunHeld = false;
        OnRunReleased?.Invoke(this, EventArgs.Empty);
    }

    private void OnJump_started(InputAction.CallbackContext ctx)
    {
        IsJumpPressed = true;
        OnJumpPressed?.Invoke(this, EventArgs.Empty);
    }

    private void OnJump_canceled(InputAction.CallbackContext ctx)
    {
        OnJumpReleased?.Invoke(this, EventArgs.Empty);
        IsJumpReleased = true;
    }
    #endregion
}

// ─────────────────────────────────────────────────────────────────────────────
//  Event argument helper classes
// ─────────────────────────────────────────────────────────────────────────────
public sealed class MoveArgs : EventArgs
{
    public Vector2 MoveDir { get; }
    public MoveArgs(Vector2 dir) => MoveDir = dir;
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
