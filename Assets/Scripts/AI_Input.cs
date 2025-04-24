using UnityEngine;

public class AI_Input : IInputProvider
{
    private readonly CharacterController _cc;
    private Transform _player;

    public AI_Input(CharacterController cc, Transform player)
    {
        _cc = cc;
        _player = player;
    } 

    // === proprietà IInputProvider ===
    public Vector2 MoveDir { get; private set; }
    public bool IsRunHeld { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsJumpReleased { get; private set; }

    // ==============================================================
    // Esempio di logica: segui il giocatore se è nel raggio, salta ostacoli
    // ==============================================================

    public void Tick()
    {
        float dist = Vector2.Distance(_player.position, _cc.transform.position);

        // Movimento orizzontale
        if (dist < 10f) MoveDir = (_player.position - _cc.transform.position).normalized;
        else MoveDir = Vector2.zero;

        // Corsa quando vicino
        IsRunHeld = dist < 5f;

        // Semplice salto se ostacolo davanti ed è grounded
        bool muroDavanti = Physics2D.Raycast(_cc.transform.position,
                                             Vector2.right * Mathf.Sign(MoveDir.x),
                                             0.75f,
                                             LayerMask.GetMask("Ground"));
        IsJumpPressed = muroDavanti && _cc.IsGrounded;
        IsJumpReleased = !IsJumpPressed;    // banalissimo esempio
    }
}
