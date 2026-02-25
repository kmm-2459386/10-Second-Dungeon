using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("体力")]
    [SerializeField] private int maxHP = 100;
    private int currentHP;

    [Header("Animator")]
    [SerializeField] private Animator anim; // プレイヤーのAnimator

    public bool IsDead { get; private set; } = false;

    void Start()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log("Player Damage: " + damage + " / Current HP: " + currentHP);

        // ダメージアニメーション
        if (anim != null)
        {
            anim.SetTrigger("Damage");
        }

        // HPが0になったら死亡処理
        if (currentHP <= 0)
        {
            Die();
        }
    }
    public int GetCurrentHPMax()
    {
        return maxHP;
    }
    /// <summary>
    /// プレイヤー死亡処理
    /// </summary>
    private void Die()
    {
        if (IsDead) return;

        IsDead = true;
        Debug.Log("Player Died");

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 移動や操作を停止する場合はここで管理
        // 例: PlayerController を無効化
        // GetComponent<PlayerController>().enabled = false;
    }

    /// <summary>
    /// 現在HP取得
    /// </summary>
    public int GetCurrentHP()
    {
        return currentHP;
    }
}