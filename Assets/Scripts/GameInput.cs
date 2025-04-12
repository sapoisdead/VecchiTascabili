using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    private Input _input;

    public event EventHandler OnJump; 

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one GAMEINPUT in the current scene");
            Destroy(gameObject);
        }
        else { Instance = this; DontDestroyOnLoad(gameObject); }
        _input = new Input();
        _input.Enable();

        _input.Player.Jump.performed += Jump_performed;
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        Debug.Log("JUMP!");
        OnJump?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMoveDirNormalized()
    {
        Vector2 moveDir = _input.Player.Move.ReadValue<Vector2>();
        return moveDir.normalized;
    }

}
