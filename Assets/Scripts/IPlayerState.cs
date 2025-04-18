public interface IPlayerState
{
    void Enter();          // chiamato una sola volta allo switch dello stato
    void Update();
    void FixedUpdate();
    void Exit();           // pulizia – chiamato quando si esce dallo stato
}