public class PlayerController : CharacterController
{
    protected override void Start()
    {
        base.Start();                       // <-- questa linea inizializza tutto il resto
        SetInputProvider(GameInput.Instance);
    }
}