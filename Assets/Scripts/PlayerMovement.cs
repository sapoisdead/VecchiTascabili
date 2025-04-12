using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rb;

    private void Start()
    {
        GameInput.Instance.OnJump += Handle_OnJump;
    }

    private void OnDisable()
    {
        GameInput.Instance.OnJump -= Handle_OnJump;   
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>(); 
    }

    private void Handle_OnJump(object sender, System.EventArgs e)
    {
        _rb.AddForce(Vector2.up * 20f, ForceMode2D.Impulse);
    }
}
