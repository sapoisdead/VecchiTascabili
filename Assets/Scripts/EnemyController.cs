using UnityEngine;

public class EnemyController : CharacterController
{
    private AI_Input _ai;
    [SerializeField] private Transform _player;

    protected override void Start()
    {
        base.Start();
        _ai = new AI_Input(this, _player);
        SetInputProvider(_ai);
    }
    protected override void Update()
    {
        base.Update();
        _ai.Tick();
    }  // Aggiorna decisioni AI
}