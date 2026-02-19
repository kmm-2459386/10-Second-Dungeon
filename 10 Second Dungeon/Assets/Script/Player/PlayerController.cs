using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float airAcceleration = 40f;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 14f;
    [SerializeField] private float gravity = -35f;
    [SerializeField] private float fallMultiplier = 1.7f;

    [Header("Assist")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    private float horizontal;
    private Vector2 velocity;

    private float coyoteCounter;
    private float jumpBufferCounter;

    private bool isGrounded;

    void Update()
    {
        velocity = rb.linearVelocity;   // ← これ追加

        HandleInput();
        CheckGround();
        HandleJump();
        HandleMovement();
        ApplyGravity();

        rb.linearVelocity = velocity;
    }

    void HandleInput()
    {
        // 横入力
        horizontal = 0;
        if (Input.GetKey(KeyCode.A)) horizontal = -1;
        if (Input.GetKey(KeyCode.D)) horizontal = 1;

        // ジャンプバッファ
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            0.1f,
            groundLayer
        );

        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;
    }

    void HandleJump()
    {
        // ジャンプ発動
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            velocity.y = jumpPower;
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }

        // 可変ジャンプ
        if (Input.GetKeyUp(KeyCode.Space) && velocity.y > 0)
        {
            velocity.y *= 0.5f;
        }
    }

    void HandleMovement()
    {
        if (isGrounded)
        {
            velocity.x = horizontal * moveSpeed;
        }
        else
        {
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                horizontal * moveSpeed,
                airAcceleration * Time.deltaTime
            );
        }
    }

    void ApplyGravity()
    {
        if (velocity.y < 0)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}
