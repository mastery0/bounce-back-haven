
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player_Controller : MonoBehaviour
{
    public Player_Controls Inputs;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 7f;

    public List<GameObject> checks; // 0 ground, 1 left wall, 2 right wall
    public LayerMask groundLayer;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 2f;

    [Header("Wall Jump")]
    public float wallJumpForceX = 8f;
    public float wallJumpForceY = 8f;
    public float wallJumpLockTime = 0.2f;

    private Rigidbody2D rb;
    private Vector2 movement;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;

    public bool IsGrounded => isGrounded;
    public bool IsTouchingWall => isTouchingWall;
    public bool IsWallSliding => isWallSliding;

    private int wallJumpDirection;
    private float wallJumpTimer;

    void Awake()
    {
        Inputs = new Player_Controls();
    }

    void OnEnable()
    {
        Inputs.Enable();
        Inputs.Player.Jump.performed += ctx => OnJump();
    }

    void OnDisable()
    {
        Inputs.Player.Jump.performed += ctx =>OnJump();
        Inputs.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (wallJumpTimer > 0) wallJumpTimer -= Time.deltaTime;

        CheckGround();
        CheckWalls();
    }

    void FixedUpdate()
    {
        OnMove();

        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < 0;

        if (isWallSliding) rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
    }

    void CheckGround()
    {
        Collider2D check = checks[0].GetComponent<Collider2D>();
        isGrounded = Physics2D.OverlapBox(check.bounds.center, check.bounds.size, 0, groundLayer);
    }

    void CheckWalls()
    {
        Collider2D left = checks[1].GetComponent<Collider2D>();
        Collider2D right = checks[2].GetComponent<Collider2D>();

        bool leftWall = Physics2D.OverlapBox(left.bounds.center, left.bounds.size, 0, groundLayer);
        bool rightWall = Physics2D.OverlapBox(right.bounds.center, right.bounds.size, 0, groundLayer);

        isTouchingWall = leftWall || rightWall;

        if (leftWall) wallJumpDirection = 1;
        else if (rightWall) wallJumpDirection = -1;
    }

    public void OnMove()
    {
        movement = Inputs.Player.Move.ReadValue<Vector2>();

        if (wallJumpTimer <= 0) rb.linearVelocity = new Vector2(movement.x * speed, rb.linearVelocity.y);
    }

    public void OnJump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
        else if (isTouchingWall)
        {
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpForceX, wallJumpForceY);
            wallJumpTimer = wallJumpLockTime;
            isTouchingWall = false;
            isWallSliding = false;
        }
    }

    void OnDrawGizmos()
    {
        if (checks == null || checks.Count < 3) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(checks[0].GetComponent<Collider2D>().bounds.center, checks[0].GetComponent<Collider2D>().bounds.size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(checks[1].GetComponent<Collider2D>().bounds.center, checks[1].GetComponent<Collider2D>().bounds.size);
        Gizmos.DrawWireCube(checks[2].GetComponent<Collider2D>().bounds.center, checks[2].GetComponent<Collider2D>().bounds.size);
    }
}
