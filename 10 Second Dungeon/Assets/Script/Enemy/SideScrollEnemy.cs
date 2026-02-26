using UnityEngine;

public class SideScrollEnemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack, AttackCooldown }
    private EnemyState currentState;

    [Header("移動")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float chaseSpeed = 3f;

    [Header("検知")]
    [SerializeField] float detectDistance = 5f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] LayerMask playerLayer;

    [Header("壁検知")]
    [SerializeField] float wallCheckDistance = 0.5f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform wallCheckPoint;

    [Header("崖検知")]
    [SerializeField] float groundCheckDistance = 1f;
    [SerializeField] Transform groundCheckPoint;

    [Header("攻撃判定")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private int attackDamage = 10;

    [Header("攻撃クールタイム")]
    [SerializeField] private float attackCooldown = 1.0f;
    private float lastAttackTime = -999f;

    private Rigidbody2D rb;
    private Transform player;
    private int direction = 1;
    private bool canMove = true;
    [SerializeField] private Animator anim;  // 子のAnimator
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        ChangeState(EnemyState.Patrol);
    }

    void Update()
    {
       // Debug.Log(currentState);
        switch (currentState)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Chase: Chase(); break;
            case EnemyState.Attack: Attack(); break;
            case EnemyState.AttackCooldown: AttackCooldown(); break;
        }
        UpdateAnimation();
        //Debug.Log("Real Velocity: " + rb.linearVelocity.x);
    }

    // 状態変更
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
       // Debug.Log("現在の状態: " + currentState);
    }

    // 巡回
    void Patrol()
    {
        if (!canMove) return;

        rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);

        if (IsWallAhead() || IsCliffAhead())
        {
            Flip();
            return;
        }

        if (DetectPlayer())
            ChangeState(EnemyState.Chase);
    }

    // 追跡
    void Chase()
    {
        if (!canMove) return;

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);

        if (InAttackRange())
            ChangeState(EnemyState.Attack);

        if (!DetectPlayer())
            ChangeState(EnemyState.Patrol);
    }

    // 攻撃
    void Attack()
    {
        // 射程外なら戻る
        if (!InAttackRange())
        {
            isAttacking = false;
            canMove = true;
            ChangeState(EnemyState.Chase);
            return;
        }

        // クールタイム中
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        if (!isAttacking)
        {
            isAttacking = true;
            canMove = false;   // ← ここで停止確定
            rb.linearVelocity = Vector2.zero;
            anim.SetTrigger("Attack");
        }
    }
    // AnimationEventから呼ぶ攻撃判定
    public void DealDamage()
    {
        // 攻撃クールタイム判定
        if (Time.time - lastAttackTime < attackCooldown) return;

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
               // Debug.Log($"Enemy dealt {attackDamage} damage to player");
            }
        }

        lastAttackTime = Time.time;
    }

    // AnimationEventから呼ぶ攻撃終了
    public void EndAttack()
    {
        isAttacking = false;
        lastAttackTime = Time.time;
        canMove = true;   // ← 再移動可能
        ChangeState(EnemyState.AttackCooldown);
    }
    void AttackCooldown()
    {
        rb.linearVelocity = Vector2.zero;

        // クールタイム終了までIdle固定
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            if (InAttackRange())
                ChangeState(EnemyState.Attack);
            else
                ChangeState(EnemyState.Chase);
        }
    }
    // アニメーション更新
    void UpdateAnimation()
    {
        bool isRunning = (currentState == EnemyState.Chase || currentState == EnemyState.Patrol);
        anim.SetBool("Run", isRunning);
    }

    // プレイヤー検知
    bool DetectPlayer()
    {
        Vector2 dir = Vector2.right * direction;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dir,
            detectDistance,
            playerLayer
        );
        return hit.collider != null;
    }

    bool InAttackRange()
    {
        return Mathf.Abs(player.position.x - transform.position.x) < attackRange;
    }

    // 壁判定
    bool IsWallAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(wallCheckPoint.position, Vector2.right * direction, wallCheckDistance, groundLayer);
        return hit.collider != null;
    }

    void Flip()
    {
        direction *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        //Debug.Log("壁を検知 → 反転");
    }

    // 崖判定
    bool IsCliffAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider == null;
    }

    void OnDrawGizmosSelected()
    {
        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(wallCheckPoint.position, wallCheckPoint.position + Vector3.right * direction * wallCheckDistance);
        }
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}