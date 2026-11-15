
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour
{
    public Player_Controls Inputs;

    public float speed=5f;
    public float jumpForce=7f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded=true;


    void Awake()
    {
        Inputs = new();
        Inputs.Enable();

        Inputs.Player.Jump.performed += ctx => OnJump();
      
    }
    void Start()
    {
        rb=GetComponent<Rigidbody2D>();
    }

    public void OnMove()
    {
        movement=Inputs.Player.Move.ReadValue<Vector2>();
        rb.linearVelocity = new Vector2(movement.x * speed,
            rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
 OnMove();
        rb.linearVelocity=new Vector2(
            movement.x*speed,
            rb.linearVelocity.y
        );
    }

    public void OnJump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
            isGrounded=false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded=true;
        }
    }
}
