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

    [Header("Dash")]
    public float dashPower = 20f;
    public float dashTime = 0.2f;
    public float dashCooldown = 0.5f;

    private bool isDashing;
    private bool hasAirDashed;
    private float dashTimeCounter;
    private float dashCooldownCounter;

    private float horizontal;
    private Vector2 velocity;

    private float coyoteCounter;
    private float jumpBufferCounter;

    private bool isGrounded;
    private Animator animator;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        velocity = rb.linearVelocity;

        // クールダウン更新
        if (dashCooldownCounter > 0)
            dashCooldownCounter -= Time.deltaTime;

        // ダッシュ入力
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownCounter <= 0)
        {
            if (isGrounded)
            {
                StartDash();
            }
            else if (!hasAirDashed)
            {
                hasAirDashed = true;
                StartDash();
            }
        }

        // ===== ダッシュ中 =====
        if (isDashing)
        {
            DashMove();
            UpdateAnimator();
            return;
        }

        // ===== 通常処理 =====
        HandleInput();
        CheckGround();
        HandleJump();
        HandleMovement();
        ApplyGravity();

        rb.linearVelocity = velocity;

        UpdateAnimator();
    }

    void HandleInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

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
        {
            coyoteCounter = coyoteTime;
            hasAirDashed = false;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    void HandleJump()
    {
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            velocity.y = jumpPower;
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }

        if (Input.GetKeyUp(KeyCode.Space) && velocity.y > 0)
            velocity.y *= 0.5f;
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

        if (horizontal > 0)
            transform.localScale = Vector3.one;
        else if (horizontal < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void ApplyGravity()
    {
        if (velocity.y < 0)
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        else
            velocity.y += gravity * Time.deltaTime;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeCounter = dashTime;
        dashCooldownCounter = dashCooldown;
    }

    void DashMove()
    {
        float direction = transform.localScale.x;

        velocity.x = direction * dashPower;
        velocity.y = 0;

        dashTimeCounter -= Time.deltaTime;

        if (dashTimeCounter <= 0)
            isDashing = false;

        rb.linearVelocity = velocity;
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsDashing", isDashing);
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