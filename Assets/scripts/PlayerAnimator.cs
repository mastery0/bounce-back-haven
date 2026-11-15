using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Player_Controller controller;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Soglie")]
    [SerializeField] private float moveThreshold = 0.1f;

    private Animator animator;

    private static readonly int SpeedId = Animator.StringToHash("Speed");
    private static readonly int GroundedId = Animator.StringToHash("Grounded");

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (controller == null) controller = GetComponent<Player_Controller>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (rb == null || animator == null) return;

        float speed = Mathf.Abs(rb.linearVelocity.x);

        bool grounded = controller != null
            ? controller.IsGrounded
            : rb.linearVelocity.y > -0.01f;

        animator.SetFloat(SpeedId, speed);
        animator.SetBool(GroundedId, grounded);

        if (spriteRenderer != null && speed > moveThreshold)
        {
            spriteRenderer.flipX = rb.linearVelocity.x < 0f;
        }
    }
}
