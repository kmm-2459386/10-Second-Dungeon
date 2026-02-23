using UnityEngine;
using System.Collections;

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

    [Header("Wall Jump")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForceX = 12f;
    [SerializeField] private float wallJumpForceY = 14f;
    [SerializeField] private float wallJumpControlTime = 0.2f;

    private bool isDashing;
    private bool hasAirDashed;
    private float dashTimeCounter;
    private float dashCooldownCounter;

    [SerializeField] private TrailRenderer trail;

    [Header("Wall Jump Assist")]
    [SerializeField] private float wallCoyoteTime = 0.15f; // 壁ジャンプ許容時間
    private float wallCoyoteCounter; // 壁ジャンプ残り時間
    private float horizontal;
    private Vector2 velocity;

    private bool isCrouching;

    [Header("Slide")]
    [SerializeField] private float slideSpeed = 18f;
    [SerializeField] private float slideDuration = 0.7f;
    [SerializeField] private float minSlideSpeed = 1f; // これ未満なら発動しない

    private bool isSliding;
    private float slideTimer;

    [Header("Slide Collider")]
    [SerializeField] private BoxCollider2D playerCollider;

    [SerializeField] private Vector2 standingSize;
    [SerializeField] private Vector2 standingOffset;

    [SerializeField] private Vector2 slideSize;
    [SerializeField] private Vector2 slideOffset;

    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private float ceilingCheckRadius = 0.1f;
    private float coyoteCounter;
    private float jumpBufferCounter;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canMove = true;

    private Animator animator;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        if (playerCollider == null)
            playerCollider = GetComponent<BoxCollider2D>();

        standingSize = playerCollider.size;
        standingOffset = playerCollider.offset;

        if (trail != null)
            trail.emitting = false; // ← これ追加
    }

    void Update()
    {
        velocity = rb.linearVelocity;

        // クールダウン更新
        if (dashCooldownCounter > 0) dashCooldownCounter -= Time.deltaTime;

        // ダッシュ入力
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownCounter <= 0)
        {
            if (isGrounded)
                StartDash();
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
        CheckWall();
        HandleWallSlide();
        HandleJump();
        HandleMovement();
        ApplyGravity();
        HandleSlide();

        rb.linearVelocity = velocity;
        UpdateAnimator();
    }

    void HandleInput()
    {
        if (!canMove) horizontal = 0;
        else horizontal = Input.GetAxisRaw("Horizontal");

        // しゃがみ入力
        bool crouchPressed = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
        bool crouchHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        // 走り中ならスライド開始
        if (crouchPressed && isGrounded && Mathf.Abs(velocity.x) > minSlideSpeed && !isSliding)
        {
            StartSlide();
            return;
        }

        // 通常しゃがみ
        isCrouching = crouchHeld && !isSliding;

        // ジャンプバッファ
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            hasAirDashed = false;
        }
        else coyoteCounter -= Time.deltaTime;
    }

    void CheckWall()
    {
        isTouchingWall = Physics2D.Raycast(
            wallCheck.position,
            transform.localScale.x > 0 ? Vector2.right : Vector2.left,
            wallCheckDistance,
            wallLayer
        );

        if (isTouchingWall && velocity.y < 0) // 壁接触中＆落下中
        {
            wallCoyoteCounter = wallCoyoteTime; // 壁ジャンプ可能時間リセット
        }
        else
        {
            wallCoyoteCounter -= Time.deltaTime; // 時間経過で減少
        }
    }

    void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && velocity.y < 0)
        {
            isWallSliding = true;
            velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
        }
        else
            isWallSliding = false;

        animator.SetBool("IsWallSliding", isWallSliding);
    }

    void HandleJump()
    {
        // 通常ジャンプ
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            velocity.y = jumpPower;
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }

        // 壁ジャンプ（コヨーテタイム対応）
        if (jumpBufferCounter > 0 && wallCoyoteCounter > 0)
        {
            StartCoroutine(WallJump());
            jumpBufferCounter = 0;
            wallCoyoteCounter = 0;
        }

        // 高さを抑える
        if (Input.GetKeyUp(KeyCode.Space) && velocity.y > 0)
            velocity.y *= 0.5f;
    }

    IEnumerator WallJump()
    {
        canMove = false;
        velocity.x = -transform.localScale.x * wallJumpForceX;
        velocity.y = wallJumpForceY;
        isWallSliding = false;

        transform.localScale = new Vector3(-transform.localScale.x, 1, 1); // 反対方向を向く

        yield return new WaitForSeconds(wallJumpControlTime);
        canMove = true;
    }

    void HandleMovement()
    {
        if (isSliding) return;
        if (isCrouching && isGrounded)
        {
            velocity.x = 0;
            return;
        }
        if (!canMove) return;

        if (isGrounded)
            velocity.x = horizontal * moveSpeed;
        else
            velocity.x = Mathf.MoveTowards(velocity.x, horizontal * moveSpeed, airAcceleration * Time.deltaTime);

        if (horizontal > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void ApplyGravity()
    {
        if (velocity.y < 0) velocity.y += gravity * fallMultiplier * Time.deltaTime;
        else velocity.y += gravity * Time.deltaTime;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeCounter = dashTime;
        dashCooldownCounter = dashCooldown;
        trail.emitting = true;
    }
    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;

        animator.SetBool("IsSliding", true);

        velocity.x = transform.localScale.x * slideSpeed;

        // コライダー縮小
        playerCollider.size = slideSize;
        playerCollider.offset = slideOffset;

    }
    bool CanStandUp()
    {
        return !Physics2D.OverlapCircle(
            ceilingCheck.position,
            ceilingCheckRadius,
            groundLayer
        );
    }
    void HandleSlide()
    {
        if (!isSliding) return;

        slideTimer -= Time.deltaTime;

        // 徐々に減速させたい場合
        velocity.x = Mathf.Lerp(velocity.x, 0, 4f * Time.deltaTime);

        if (slideTimer <= 0 || Mathf.Abs(velocity.x) < 0.5f || !isGrounded)
        {
            EndSlide();
        }
    }

    void EndSlide()
    {
        if (!CanStandUp())
            return; // 頭がぶつかるなら終了しない

        isSliding = false;
        animator.SetBool("IsSliding", false);

        // コライダー戻す
        playerCollider.size = standingSize;
        playerCollider.offset = standingOffset;
    }
    void DashMove()
    {
        float direction = transform.localScale.x;
        velocity.x = direction * dashPower;
        velocity.y = 0;

        dashTimeCounter -= Time.deltaTime;

        if (dashTimeCounter <= 0)
        {
            isDashing = false;
            trail.emitting = false;
        }
        rb.linearVelocity = velocity;
    }

    void UpdateAnimator()
    {
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsDashing", isDashing);
        animator.SetBool("IsCrouching", isCrouching);

    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Vector3 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir * wallCheckDistance);
        }
        {
            if (ceilingCheck != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
            }
        }
    }
}