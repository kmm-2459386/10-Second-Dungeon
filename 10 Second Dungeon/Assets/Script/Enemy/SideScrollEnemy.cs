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
    [Header("崖検知")]
    [SerializeField] float groundCheckDistance = 1f;
    [SerializeField] Transform groundCheckPoint;
    private Rigidbody2D rb;
    private Transform player;
    private int direction = 1;

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
    }

    // ===== 状態変更管理 =====
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        Debug.Log("現在の状態: " + currentState);
    }

    // ===== Patrol =====
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


    // ===== Chase =====
    void Chase()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);

        if (InAttackRange())
            ChangeState(EnemyState.Attack);

        if (!DetectPlayer())
            ChangeState(EnemyState.Patrol);
    }

    // ===== Attack =====
    void Attack()
    {
        rb.linearVelocity = Vector2.zero;

        if (!InAttackRange())
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 攻撃処理を書く場所
    }

    // ===== 検知 =====
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
    bool IsWallAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
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
    bool IsCliffAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider == null; // 地面が無ければ崖
    }
    void OnDrawGizmosSelected()
    {
        // 壁チェック
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.right * direction * wallCheckDistance
        );

        // 崖チェック
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
