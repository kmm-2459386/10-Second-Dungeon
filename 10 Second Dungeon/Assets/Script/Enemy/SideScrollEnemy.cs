using UnityEngine;

public class SideScrollEnemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack }
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

    private Rigidbody2D rb;
    private Transform player;
    private int direction = 1;

    [SerializeField] private Animator anim;  // 子のAnimatorをインスペクターでセット
    private bool isAttacking = false;        // 攻撃中フラグ

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        ChangeState(EnemyState.Patrol);
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Chase: Chase(); break;
            case EnemyState.Attack: Attack(); break;
        }
        UpdateAnimation();
    }

    // ===== 状態変更管理 =====
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        Debug.Log("現在の状態: " + currentState);
    }

    // ===== 巡回 =====
    void Patrol()
    {
        rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);

        if (IsWallAhead() || IsCliffAhead())
        {
            Flip();
            return;
        }

        if (DetectPlayer())
            ChangeState(EnemyState.Chase);
    }

    // ===== 追跡 =====
    void Chase()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);

        if (InAttackRange())
            ChangeState(EnemyState.Attack);

        if (!DetectPlayer())
            ChangeState(EnemyState.Patrol);
    }

    // ===== 攻撃 =====
    void Attack()
    {
        rb.linearVelocity = Vector2.zero;

        if (!isAttacking)
        {
            isAttacking = true;
            anim.SetTrigger("Attack"); // 攻撃アニメーション発火
            Debug.Log("Attack Animation Triggered");
        }

        // プレイヤーが射程外になったら Chase に戻す
        if (!InAttackRange())
        {
            isAttacking = false;
            ChangeState(EnemyState.Chase);
        }
    }

    // ===== アニメーション更新 =====
    void UpdateAnimation()
    {
        if (!isAttacking)
        {
            float speed = Mathf.Abs(rb.linearVelocity.x);
            anim.SetFloat("Speed", speed);
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        Debug.Log("Attack Animation Ended");
    }

    // ===== プレイヤー検知 =====
    bool DetectPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            transform.right * direction,
            detectDistance,
            playerLayer
        );

        return hit.collider != null;
    }

    bool InAttackRange()
    {
        return Mathf.Abs(player.position.x - transform.position.x) < attackRange;
    }

    // ===== 壁検知 =====
    bool IsWallAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheckPoint.position,
            Vector2.right * direction,
            wallCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    void Flip()
    {
        direction *= -1;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        Debug.Log("壁を検知 → 反転");
    }

    // ===== 崖検知 =====
    bool IsCliffAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider == null; // 地面がなければ崖
    }

    void OnDrawGizmosSelected()
    {
        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                wallCheckPoint.position,
                wallCheckPoint.position + Vector3.right * direction * wallCheckDistance
            );
        }

        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                groundCheckPoint.position,
                groundCheckPoint.position + Vector3.down * groundCheckDistance
            );
        }
    }
}